using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [Header("面板")]
    public GameObject startPanel;
    public GameObject gamePanel;
    public GameObject gameOverPanel;

    [Header("文本")]
    public Text scoreText;
    public Text highScoreText;
    public Text startTitleText;
    public Text startHighScoreText;
    public Text finalScoreText;
    public Text newRecordText;

    [Header("下一个水果预览")]
    public Image nextFruitImage;

    [Header("动画参数")]
    [SerializeField] private float panelFadeDuration = 0.3f;
    [SerializeField] private float panelScaleDuration = 0.35f;
    [SerializeField] private float childStaggerDelay = 0.08f; // 子元素依次入场间隔
    [SerializeField] private float scorePunchScale = 0.25f;
    [SerializeField] private float nextFruitSwapDuration = 0.25f;

    private CanvasGroup startCanvasGroup;
    private CanvasGroup gameCanvasGroup;
    private CanvasGroup gameOverCanvasGroup;
    private Sequence currentTransition;

    private void Awake()
    {
        EnsureCanvasGroup(ref startCanvasGroup, startPanel);
        EnsureCanvasGroup(ref gameCanvasGroup, gamePanel);
        EnsureCanvasGroup(ref gameOverCanvasGroup, gameOverPanel);
    }

    private void EnsureCanvasGroup(ref CanvasGroup cg, GameObject panel)
    {
        if (panel == null) return;
        cg = panel.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = panel.AddComponent<CanvasGroup>();
        }
    }

    // ==================== 面板切换 ====================

    public void ShowStartPanel(int highScore)
    {
        UpdateStartHighScore(highScore);
        TransitionTo(startPanel, startCanvasGroup, true);
        PlayTitleBreathe();
    }

    public void ShowGamePanel()
    {
        TransitionTo(gamePanel, gameCanvasGroup, false);
    }

    public void ShowGameOverPanel(int finalScore, int highScore)
    {
        if (finalScoreText != null)
            finalScoreText.text = "最终分数: " + finalScore;

        if (newRecordText != null)
        {
            bool isNewRecord = finalScore >= highScore && finalScore > 0;
            newRecordText.gameObject.SetActive(isNewRecord);
        }

        TransitionTo(gameOverPanel, gameOverCanvasGroup, true, () =>
        {
            if (newRecordText != null && newRecordText.gameObject.activeSelf)
            {
                newRecordText.transform.localScale = Vector3.zero;
                newRecordText.transform.DOScale(1f, 0.4f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(0.2f);
            }
        });
    }

    /// <summary>
    /// 核心面板切换：当前面板淡出 → 目标面板淡入+缩放弹入
    /// </summary>
    private void TransitionTo(GameObject targetPanel, CanvasGroup targetCg, bool animateChildren, System.Action onComplete = null)
    {
        // 清除之前的过渡动画
        currentTransition?.Kill();

        if (targetPanel == null) return;

        targetPanel.SetActive(true);
        if (targetCg != null)
        {
            targetCg.alpha = 0f;
            targetCg.DOFade(1f, panelFadeDuration).SetUpdate(true);
        }

        // 对目标面板做缩放弹入
        Transform targetTransform = targetPanel.transform;
        targetTransform.localScale = Vector3.one * 0.9f;
        targetTransform.DOScale(1f, panelScaleDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);

        // 隐藏其他面板
        HidePanel(startPanel, startCanvasGroup, targetPanel);
        HidePanel(gamePanel, gameCanvasGroup, targetPanel);
        HidePanel(gameOverPanel, gameOverCanvasGroup, targetPanel);

        // 子元素依次入场
        if (animateChildren)
        {
            AnimateChildrenSequence(targetPanel, targetTransform);
        }
    }

    private void HidePanel(GameObject panel, CanvasGroup cg, GameObject except)
    {
        if (panel == null || panel == except) return;
        if (cg != null)
        {
            cg.DOKill();
            cg.DOFade(0f, panelFadeDuration * 0.5f)
                .SetUpdate(true)
                .OnComplete(() => panel.SetActive(false));
        }
        else
        {
            panel.SetActive(false);
        }
    }

    /// <summary>
    /// 面板子元素依次入场：从下往上淡入，带交错延迟
    /// </summary>
    private void AnimateChildrenSequence(GameObject panel, Transform panelTransform)
    {
        int childCount = panelTransform.childCount;
        if (childCount == 0) return;

        currentTransition = DOTween.Sequence();

        for (int i = 0; i < childCount; i++)
        {
            Transform child = panelTransform.GetChild(i);
            CanvasGroup childCg = child.GetComponent<CanvasGroup>();
            if (childCg == null) childCg = child.gameObject.AddComponent<CanvasGroup>();

            Vector3 targetPos = child.localPosition;
            child.localPosition = targetPos + new Vector3(0, -30, 0);
            childCg.alpha = 0f;

            float delay = i * childStaggerDelay;

            currentTransition.Insert(delay,
                child.DOLocalMove(targetPos, 0.35f).SetEase(Ease.OutCubic));
            currentTransition.Insert(delay,
                childCg.DOFade(1f, 0.3f));
        }

        currentTransition.OnComplete(() => currentTransition = null);
    }

    // ==================== 分数更新 ====================

    public void UpdateScore(int score, int highScore)
    {
        bool isNewHighScore = score > 0 && score >= highScore;

        if (scoreText != null)
        {
            scoreText.text = score.ToString();
            scoreText.transform.DOKill();
            scoreText.transform.localScale = Vector3.one;
            scoreText.transform.DOPunchScale(Vector3.one * scorePunchScale, 0.3f, 6);

            // 分数变色：白色 → 金色 → 白色，增强反馈感
            scoreText.DOColor(new Color(1f, 0.85f, 0.3f), 0.15f)
                .OnComplete(() => scoreText.DOColor(Color.white, 0.25f));
        }

        if (highScoreText != null)
        {
            highScoreText.text = "最高分: " + highScore;
        }

        // 打破最高分时播放闪烁效果
        if (isNewHighScore)
        {
            PlayHighScoreBreak();
        }
    }

    public void UpdateStartHighScore(int highScore)
    {
        if (startHighScoreText != null)
        {
            startHighScoreText.text = "最高分: " + highScore;
        }
    }

    // ==================== 下一个水果预览 ====================

    public void UpdateNextFruitPreview(Fruit fruitPrefab)
    {
        if (nextFruitImage == null || fruitPrefab == null) return;

        SpriteRenderer sr = fruitPrefab.GetComponentInChildren<SpriteRenderer>();
        if (sr == null) return;

        // 缩放消失 → 换图 → 弹入
        nextFruitImage.transform.DOKill();
        nextFruitImage.transform.DOScale(0.5f, nextFruitSwapDuration * 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                nextFruitImage.sprite = sr.sprite;
                nextFruitImage.preserveAspect = true;
                nextFruitImage.SetNativeSize();
                nextFruitImage.rectTransform.sizeDelta *= 0.6f;

                nextFruitImage.transform.DOScale(1f, nextFruitSwapDuration * 0.5f)
                    .SetEase(Ease.OutBack);
            });
    }

    private void OnDestroy()
    {
        currentTransition?.Kill();
    }

    // ==================== 文字动效 ====================

    /// <summary>
    /// 开始面板标题呼吸效果：无限循环的轻微缩放，吸引注意力但不干扰阅读。
    /// 使用 Ease.InOutSine 确保缓动平滑自然，LoopType.Yoyo 实现往复运动。
    /// </summary>
    private void PlayTitleBreathe()
    {
        if (startTitleText == null) return;
        startTitleText.transform.DOKill();
        startTitleText.transform.localScale = Vector3.one;
        startTitleText.transform.DOScale(1.05f, 1.2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    /// <summary>
    /// 打破最高分时的闪烁效果：文字短暂变金色 + 放大脉冲。
    /// 使用 Sequence 确保颜色变化和缩放动画同步，完成后恢复白色。
    /// </summary>
    private void PlayHighScoreBreak()
    {
        if (highScoreText == null) return;

        Sequence seq = DOTween.Sequence();
        seq.Append(highScoreText.DOColor(new Color(1f, 0.85f, 0.3f), 0.15f));
        seq.Join(highScoreText.transform.DOPunchScale(Vector3.one * 0.15f, 0.4f, 8));
        seq.Append(highScoreText.DOColor(Color.white, 0.4f));
    }
}