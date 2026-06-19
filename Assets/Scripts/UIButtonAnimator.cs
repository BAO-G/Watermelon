using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// 为 UI 按钮添加悬停、点击反馈动画。
/// 直接挂载到任意 Button 所在的 GameObject 上即可生效。
/// 所有动画时长控制在 0.15-0.3s 范围内，确保响应灵敏。
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("缩放")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float pressScale = 0.95f;
    [SerializeField] private float scaleDuration = 0.15f;

    [Header("透明度")]
    [SerializeField] private float hoverAlpha = 0.85f;
    [SerializeField] private float pressAlpha = 0.7f;
    [SerializeField] private float fadeDuration = 0.15f;

    private Button button;
    private CanvasGroup canvasGroup;
    private Vector3 originalScale;
    private bool isPressed;

    private void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;

        transform.DOKill();
        transform.DOScale(originalScale * hoverScale, scaleDuration)
            .SetEase(Ease.OutBack, 0.5f);
        canvasGroup.DOFade(hoverAlpha, fadeDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!button.interactable) return;

        transform.DOKill();
        transform.DOScale(originalScale, scaleDuration)
            .SetEase(Ease.OutBack, 0.5f);
        canvasGroup.DOFade(1f, fadeDuration);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;
        isPressed = true;

        transform.DOKill();
        transform.DOScale(originalScale * pressScale, scaleDuration * 0.6f)
            .SetEase(Ease.InBack);
        canvasGroup.DOFade(pressAlpha, fadeDuration * 0.6f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!button.interactable) return;
        isPressed = false;

        transform.DOKill();
        transform.DOScale(originalScale * hoverScale, scaleDuration)
            .SetEase(Ease.OutBack, 0.5f);
        canvasGroup.DOFade(hoverAlpha, fadeDuration);
    }

    private void OnDisable()
    {
        transform.DOKill();
        canvasGroup.DOKill();
    }
}