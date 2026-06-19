using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ScorePopup : MonoBehaviour
{
    private Text textComponent;

    private void Awake()
    {
        textComponent = GetComponent<Text>();
    }

    public void Play(float duration, float floatDistance)
    {
        if (textComponent == null)
        {
            Destroy(gameObject);
            return;
        }

        RectTransform rt = GetComponent<RectTransform>();
        Vector2 startPos = rt.anchoredPosition;

        textComponent.DOFade(0f, duration).SetEase(Ease.InQuad);
        rt.DOAnchorPosY(startPos.y + floatDistance, duration).SetEase(Ease.OutQuad)
            .OnComplete(() => Destroy(gameObject));

        transform.DOScale(1.3f, duration * 0.3f).SetLoops(2, LoopType.Yoyo);
    }
}
