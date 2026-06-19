using System.Collections;
using UnityEngine;
using DG.Tweening;

public enum FruitType
{
    Fruit1 = 0,
    Fruit2 = 1,
    Fruit3 = 2,
    Fruit4 = 3,
    Fruit5 = 4,
    Fruit6 = 5,
}

public enum FruitState
{
    Ready,
    Standby,
    Falling,
    Landed,
}

public class Fruit : MonoBehaviour
{
    [SerializeField] private FruitType fruitType;
    public FruitState fruitState;

    private bool isCurrent;
    private bool isReleased;
    private bool hasCombined;
    private bool hasNotifiedLand;
    private bool isCheckingLand;
    private float fallStartTime;
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private Vector3 originalScale;
    private const float FallBoost = 1.5f;
    private const float MinLandVelocity = 0.08f;
    private const float LandCheckInterval = 0.3f;
    private const float MaxFallTime = 5f; // 落地检测安全超时，防止水果长时间无法触发落地

    public FruitType FruitType => fruitType;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();

        // 设置排序顺序，确保水果渲染在地板(SortingOrder=1)之上
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 2;
        }

        originalScale = transform.localScale;

        // 默认初始化为 Ready 状态（玩家生成的水果）
        // 合并产生的水果会在 SetupAsMerged() 中覆盖此状态
        fruitState = FruitState.Ready;

        if (rb != null)
        {
            rb.gravityScale = 0f;
        }

        if (circleCollider != null)
        {
            circleCollider.enabled = false;
        }
    }

    private void Start()
    {
        // 状态初始化已移至 Awake()，此处不再重置状态
        // 以避免覆盖 SetupAsMerged() 设置的 Falling 状态
    }

    private void Update()
    {
        UpdateState();
    }

    private void FixedUpdate()
    {
        if (fruitState == FruitState.Falling && !hasNotifiedLand && !isCheckingLand && rb != null)
        {
            // 安全超时：如果水果下落时间过长，强制触发落地
            if (Time.time - fallStartTime > MaxFallTime)
            {
                ForceLand();
                return;
            }

            if (rb.velocity.sqrMagnitude < MinLandVelocity * MinLandVelocity && rb.angularVelocity < 5f)
            {
                isCheckingLand = true;
                StartCoroutine(LandCheckCoroutine());
            }
        }
    }

    private IEnumerator LandCheckCoroutine()
    {
        yield return new WaitForSeconds(LandCheckInterval);

        if (this == null || rb == null)
        {
            yield break;
        }

        if (fruitState == FruitState.Falling && rb.velocity.sqrMagnitude < MinLandVelocity * MinLandVelocity)
        {
            fruitState = FruitState.Landed;
            hasNotifiedLand = true;
            GameManager.Instance?.OnFruitLanded(this);
        }
        isCheckingLand = false;
    }

    /// <summary>
    /// 强制触发落地，作为安全超时的兜底机制
    /// </summary>
    private void ForceLand()
    {
        if (hasNotifiedLand || fruitState != FruitState.Falling)
        {
            return;
        }

        fruitState = FruitState.Landed;
        hasNotifiedLand = true;
        GameManager.Instance?.OnFruitLanded(this);
    }

    private void UpdateState()
    {
        switch (fruitState)
        {
            case FruitState.Ready:
                fruitState = FruitState.Standby;
                break;

            case FruitState.Standby:
                HandleStandby();
                break;

            case FruitState.Falling:
                break;

            case FruitState.Landed:
                break;
        }
    }

    public void SetupAsCurrent()
    {
        isCurrent = true;
        isReleased = false;
        hasNotifiedLand = false;
        fruitState = FruitState.Ready;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (circleCollider != null)
        {
            circleCollider.enabled = false;
        }
    }

    private void HandleStandby()
    {
        if (!isCurrent)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (circleCollider != null)
            {
                circleCollider.enabled = true;
            }
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float clampedX = Mathf.Clamp(worldPos.x, GameManager.Instance.LeftBound, GameManager.Instance.RightBound);
            transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
        }

        if (Input.GetMouseButtonUp(0) && !isReleased)
        {
            Release();
        }
    }

    private void Release()
    {
        isReleased = true;
        isCurrent = false;
        fruitState = FruitState.Falling;
        fallStartTime = Time.time;

        if (circleCollider != null)
        {
            circleCollider.enabled = true;
        }

        if (rb != null)
        {
            rb.gravityScale = 1f;
            rb.velocity = new Vector2(0, -FallBoost);
        }

        GameManager.Instance?.OnCurrentFruitReleased();
        AudioManager.Instance?.PlayDropSound();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
        {
            return;
        }

        if (collision.transform.CompareTag("Fruit"))
        {
            if (!collision.transform.TryGetComponent(out Fruit otherFruit))
                return;

            // 非同类型水果碰撞 → 如果是当前水果，立即生成下一个
            if (otherFruit.fruitType != fruitType)
            {
                if (IsCurrentFruit())
                    GameManager.Instance.OnCurrentFruitCollided();
                return;
            }

            // 最高级水果不再合并
            if (fruitType == FruitType.Fruit6)
                return;

            // Standby 状态的水果不参与合并（玩家正在拖拽）
            if (fruitState == FruitState.Standby || otherFruit.fruitState == FruitState.Standby)
                return;

            // 只有双方都未合并过，才执行合并逻辑
            if (!hasCombined && !otherFruit.hasCombined)
            {
                hasCombined = true;
                otherFruit.hasCombined = true;

                GameManager.Instance.CombineNewFruit(fruitType, transform.position, collision.transform.position, this, otherFruit);
            }

            // ★ 关键修复：无论谁"赢得"合并，双方都被销毁，防止幽灵水果
            Destroy(gameObject);
        }
        else if (collision.transform.CompareTag("Floor"))
        {
            if (fruitState == FruitState.Falling)
            {
                AudioManager.Instance?.PlayDropSound();
            }
            // 当前水果碰撞地板 → 立即生成下一个水果
            if (IsCurrentFruit())
                GameManager.Instance.OnCurrentFruitCollided();
        }
    }

    public void PlaySpawnAnimation()
    {
        // 当前水果生成：从 0 弹入 + 轻微旋转回正
        transform.localScale = Vector3.zero;
        transform.DOScale(originalScale, 0.35f)
            .SetEase(Ease.OutBack, 0.7f);

        transform.rotation = Quaternion.Euler(0, 0, 15f);
        transform.DORotate(Vector3.zero, 0.3f)
            .SetEase(Ease.OutCubic);
    }

    public void SetupAsMerged()
    {
        // 合并产生的水果需要设置为下落状态，以便触发落地检测
        fruitState = FruitState.Falling;
        hasNotifiedLand = false;
        isCurrent = false;
        isReleased = true;
        hasCombined = false;
        fallStartTime = Time.time;

        if (rb != null)
        {
            rb.gravityScale = 1f;
        }

        if (circleCollider != null)
        {
            circleCollider.enabled = true;
        }

        // 合并动画：从 0 弹入，带更大的 overshoot 和短暂的旋转，表现"升级"感
        transform.localScale = Vector3.zero;
        Sequence mergeSeq = DOTween.Sequence();
        mergeSeq.Append(transform.DOScale(originalScale * 1.15f, 0.15f).SetEase(Ease.OutQuad));
        mergeSeq.Append(transform.DOScale(originalScale, 0.3f).SetEase(Ease.OutBack, 1.2f));

        transform.rotation = Quaternion.Euler(0, 0, Random.Range(-20f, 20f));
        transform.DORotate(Vector3.zero, 0.35f)
            .SetEase(Ease.OutCubic);
    }

    public float GetRadius()
    {
        return circleCollider != null ? circleCollider.radius * transform.localScale.x : 0.5f;
    }

    /// <summary>
    /// 判断此水果是否为 GameManager 中记录的当前玩家控制水果。
    /// 使用实例引用比较，确保只有玩家刚释放的水果才触发碰撞生成，
    /// 已落地的水果之间的碰撞不会触发重复生成。
    /// </summary>
    private bool IsCurrentFruit()
    {
        return GameManager.Instance != null && this == GameManager.Instance.CurrentFruit;
    }
}
