using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    Ready,
    InProgress,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("水果 Prefab")]
    [SerializeField] private Fruit[] fruits;

    [Header("生成参数")]
    [Tooltip("水果生成位置")]
    public Transform ftuirTransform;
    [SerializeField] private float spawnDelay = 0.6f;
    [SerializeField] private int maxSpawnableIndex = 3;

    [Header("边界设置")]
    [SerializeField] private float leftBound = -3.3f;
    [SerializeField] private float rightBound = 3.3f;

    [Header("死亡线")]
    [SerializeField] private float deathLineY = 2.5f;
    [SerializeField] private float deathLineStayDuration = 2f;

    [Header("UI 设置（可选，未赋值则自动查找）")]
    public Button startButton;
    public Button restartButton;
    public UIManager uiManager;

    public GameState CurrentState { get; private set; } = GameState.Ready;
    public int CurrentScore { get; private set; }
    public int HighScore { get; private set; }
    public int NextFruitIndex { get; private set; }

    public float LeftBound => leftBound;
    public float RightBound => rightBound;
    public float DeathLineY => deathLineY;
    public float DeathLineStayDuration => deathLineStayDuration;
    public bool IsGameOver => CurrentState == GameState.GameOver;
    public bool IsPlaying => CurrentState == GameState.InProgress;

    private Fruit currentFruit;
    public Fruit CurrentFruit => currentFruit;  // 只读属性，供外部判断当前水果引用
    private Coroutine spawnCoroutine;
    private const string HighScoreKey = "WatermelonHighScore";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (fruits == null || fruits.Length == 0)
        {
            Debug.LogError("[GameManager] fruits 数组为空，请在 Inspector 中赋值水果 Prefab");
            return;
        }

        if (ftuirTransform == null)
        {
            Debug.LogError("[GameManager] ftuirTransform 未赋值，请在 Inspector 中赋值生成位置");
            return;
        }

        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }

        EnsureDeathLine();
        EnsureManagers();

        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        PrepareNextFruit();
        ShowStartState();

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    private void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(StartGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
        }
    }

    public void StartGame()
    {
        if (CurrentState != GameState.Ready)
        {
            return;
        }

        CurrentState = GameState.InProgress;
        CurrentScore = 0;
        currentFruit = null;
        StopSpawnCoroutine();

        UpdateUI();
        uiManager?.ShowGamePanel();

        SpawnNextFruit();
    }

    public void RestartGame()
    {
        ClearAllFruits();
        CurrentState = GameState.Ready;
        CurrentScore = 0;
        currentFruit = null;
        StopSpawnCoroutine();
        PrepareNextFruit();
        ShowStartState();
    }

    private void ClearAllFruits()
    {
        Fruit[] allFruits = FindObjectsOfType<Fruit>();
        foreach (Fruit fruit in allFruits)
        {
            if (fruit != null)
            {
                Destroy(fruit.gameObject);
            }
        }
    }

    private void PrepareNextFruit()
    {
        int maxIndex = Mathf.Min(maxSpawnableIndex, fruits.Length);
        if (maxIndex <= 0)
        {
            Debug.LogError("[GameManager] 无可生成水果：fruits 数组为空或 maxSpawnableIndex <= 0");
            return;
        }
        NextFruitIndex = UnityEngine.Random.Range(0, maxIndex);
        uiManager?.UpdateNextFruitPreview(fruits[NextFruitIndex]);
    }

    /// <summary>
    /// 立即生成下一个水果（由 StartGame 或安全网直接调用，不经过延迟）
    /// </summary>
    public void SpawnNextFruit()
    {
        if (CurrentState != GameState.InProgress)
            return;

        // 清除任何待处理的调度协程
        StopSpawnCoroutine();

        // 检查 currentFruit 引用是否有效
        if (currentFruit != null)
        {
            if (currentFruit.gameObject == null)
                currentFruit = null;
            else
                return; // 当前水果仍有效，不能生成
        }

        DoSpawn();
    }

    /// <summary>
    /// 调度延迟生成（由 OnFruitLanded 或 CombineNewFruit 调用）
    /// 使用协程替代 Invoke，失败时自动重试，不会死锁
    /// </summary>
    private void ScheduleSpawn()
    {
        if (CurrentState != GameState.InProgress)
            return;

        // 如果已有协程在等待，不重复创建
        if (spawnCoroutine != null)
            return;

        spawnCoroutine = StartCoroutine(SpawnAfterDelayRoutine());
    }

    /// <summary>
    /// 延迟生成协程：等待 spawnDelay 后持续轮询直到可以生成
    /// 相比 Invoke 的优势：
    ///   - 不会因 GameObject inactive 而静默丢失
    ///   - currentFruit 仍有效时自动重试，不会死锁
    ///   - 不需要 isWaitingToSpawn 标志位
    /// </summary>
    private System.Collections.IEnumerator SpawnAfterDelayRoutine()
    {
        yield return new WaitForSeconds(spawnDelay);

        // 等待直到可以生成（或游戏结束）
        while (CurrentState == GameState.InProgress)
        {
            // 清除已被销毁但引用未清的水果
            if (currentFruit != null && currentFruit.gameObject == null)
                currentFruit = null;

            if (currentFruit == null)
            {
                DoSpawn();
                break;
            }

            // 当前水果仍有效，等待后重试
            yield return new WaitForSeconds(0.1f);
        }

        spawnCoroutine = null;
    }

    /// <summary>
    /// 停止当前的生成协程
    /// </summary>
    private void StopSpawnCoroutine()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    /// <summary>
    /// 实际执行水果实例化（从 SpawnNextFruit 或协程调用）
    /// </summary>
    private void DoSpawn()
    {
        int index = NextFruitIndex;
        PrepareNextFruit();

        Fruit prefab = fruits[index];
        Vector3 spawnPos = ftuirTransform.position;
        currentFruit = Instantiate(prefab, spawnPos, Quaternion.identity);
        currentFruit.SetupAsCurrent();
    }

    public void OnCurrentFruitReleased()
    {
        // 不再承担生成调度职责；生成由 OnFruitLanded / OnCurrentFruitCollided / CombineNewFruit 统一触发
    }

    /// <summary>
    /// 当前水果发生碰撞时立即触发（无论碰撞对象是水果还是地板）。
    /// 直接生成下一个水果，无需等待落地检测或延迟。
    /// 使用实例引用比较确保只有玩家刚释放的水果才触发，防止重复调用。
    /// </summary>
    public void OnCurrentFruitCollided()
    {
        if (CurrentState != GameState.InProgress) return;

        currentFruit = null;
        StopSpawnCoroutine();
        SpawnNextFruit();
    }

    public void OnFruitLanded(Fruit fruit)
    {
        if (CurrentState != GameState.InProgress)
            return;

        if (fruit == null || fruit.gameObject == null)
            return;

        if (fruit == currentFruit)
            currentFruit = null;

        ScheduleSpawn();
    }

    public Fruit CombineNewFruit(FruitType fruitType, Vector2 currentPos, Vector2 collisionPos, Fruit destroyedFruit = null, Fruit otherDestroyedFruit = null)
    {
        if (CurrentState == GameState.GameOver)
        {
            return null;
        }

        int index = (int)fruitType + 1;
        if (index >= fruits.Length)
        {
            return null;
        }

        // 两个被销毁的水果中任意一个是当前玩家控制的水果，则清除引用并立即生成
        bool currentFruitDestroyed = (destroyedFruit != null && destroyedFruit == currentFruit) ||
                                     (otherDestroyedFruit != null && otherDestroyedFruit == currentFruit);

        if (currentFruitDestroyed)
        {
            // 当前水果合并销毁 → 立即生成下一个，不用等待
            OnCurrentFruitCollided();
        }
        else
        {
            // 非当前水果合并 → 仍使用延迟调度，确保生成队列不中断
            ScheduleSpawn();
        }

        Vector2 centerPos = (currentPos + collisionPos) * 0.5f;
        Fruit newFruit = Instantiate(fruits[index], centerPos, Quaternion.identity);
        newFruit.PlaySpawnAnimation();
        newFruit.SetupAsMerged();

        int score = GetMergeScore(fruitType);
        AddScore(score);

        AudioManager.Instance?.PlayMergeSound();
        EffectsManager.Instance?.PlayMergeEffect(centerPos, (int)fruitType);
        VibrationManager.Instance?.VibrateMerge();
        ScorePopupManager.Instance?.Show(centerPos, score);

        return newFruit;
    }

    private int GetMergeScore(FruitType fruitType)
    {
        return ((int)fruitType + 1) * 10;
    }

    private void AddScore(int score)
    {
        CurrentScore += score;
        if (CurrentScore > HighScore)
        {
            HighScore = CurrentScore;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        uiManager?.UpdateScore(CurrentScore, HighScore);
    }

    public void GameOver()
    {
        if (CurrentState == GameState.GameOver)
        {
            return;
        }

        CurrentState = GameState.GameOver;
        StopSpawnCoroutine();
        uiManager?.ShowGameOverPanel(CurrentScore, HighScore);
        AudioManager.Instance?.PlayGameOverSound();
        VibrationManager.Instance?.VibrateGameOver();
    }

    private void ShowStartState()
    {
        uiManager?.ShowStartPanel(HighScore);
    }

    private void Update()
    {
        // 安全网：如果游戏进行中但既没有当前水果也没有待运行的协程，
        // 说明事件链可能断开了，由轮询兜底（防御性编程）
        if (CurrentState == GameState.InProgress && currentFruit == null && spawnCoroutine == null)
        {
            ScheduleSpawn();
        }
    }

    private void EnsureDeathLine()
    {
        DeathLine existing = FindObjectOfType<DeathLine>();
        if (existing != null)
        {
            return;
        }

        GameObject deathLineObj = GameObject.Find("DeathLine");
        if (deathLineObj == null)
        {
            deathLineObj = new GameObject("DeathLine");
            deathLineObj.transform.position = new Vector3(0, deathLineY, 0);

            BoxCollider2D col = deathLineObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(rightBound - leftBound + 1f, 0.2f);
            col.isTrigger = true;

            LineRenderer lr = deathLineObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPositions(new Vector3[]
            {
                new Vector3(leftBound, 0, 0),
                new Vector3(rightBound, 0, 0)
            });
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.red;
            lr.endColor = Color.red;
            lr.useWorldSpace = false;
        }

        deathLineObj.AddComponent<DeathLine>();
    }

    private void EnsureManagers()
    {
        EnsureManager<AudioManager>();
        EnsureManager<EffectsManager>();
        EnsureManager<ScorePopupManager>();
        EnsureManager<VibrationManager>();
    }

    private void EnsureManager<T>() where T : MonoBehaviour
    {
        T existing = FindObjectOfType<T>();
        if (existing != null)
        {
            return;
        }

        GameObject managerObj = new GameObject(typeof(T).Name);
        managerObj.AddComponent<T>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 left = new Vector3(leftBound, deathLineY, 0);
        Vector3 right = new Vector3(rightBound, deathLineY, 0);
        Gizmos.DrawLine(left, right);

        Gizmos.color = Color.green;
        Vector3 topLeft = new Vector3(leftBound, ftuirTransform != null ? ftuirTransform.position.y : 5.4f, 0);
        Vector3 topRight = new Vector3(rightBound, topLeft.y, 0);
        Gizmos.DrawLine(topLeft, topRight);
    }
}
