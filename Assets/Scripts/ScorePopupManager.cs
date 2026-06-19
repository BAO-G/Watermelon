using UnityEngine;
using UnityEngine.UI;

public class ScorePopupManager : MonoBehaviour
{
    public static ScorePopupManager Instance { get; private set; }

    [Header("飘分文本预制体（Canvas 下的 Text）")]
    [SerializeField] private Text scoreTextPrefab;

    [Header("显示设置")]
    [SerializeField] private float floatDuration = 0.8f;
    [SerializeField] private float floatDistance = 50f;

    private Canvas parentCanvas;

    private void Awake()
    {
        Instance = this;
        parentCanvas = FindObjectOfType<Canvas>();
    }

    public void Show(Vector2 worldPosition, int score)
    {
        if (scoreTextPrefab == null || parentCanvas == null)
        {
            return;
        }

        Text popup = Instantiate(scoreTextPrefab, parentCanvas.transform);
        popup.text = "+" + score;

        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.GetComponent<RectTransform>(),
            screenPos,
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out Vector2 localPos);

        RectTransform rt = popup.GetComponent<RectTransform>();
        rt.anchoredPosition = localPos;

        ScorePopup anim = popup.GetComponent<ScorePopup>();
        if (anim != null)
        {
            anim.Play(floatDuration, floatDistance);
        }
        else
        {
            popup.gameObject.AddComponent<ScorePopup>().Play(floatDuration, floatDistance);
        }
    }
}
