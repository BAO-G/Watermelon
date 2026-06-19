using UnityEngine;

public class VibrationManager : MonoBehaviour
{
    public static VibrationManager Instance { get; private set; }

    [Header("震动反馈")]
    [SerializeField] private bool enableVibration = true;

    private void Awake()
    {
        Instance = this;
    }

    public void VibrateMerge()
    {
        if (!enableVibration)
        {
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate();
#elif UNITY_IOS && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }

    public void VibrateGameOver()
    {
        if (!enableVibration)
        {
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate();
#elif UNITY_IOS && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }
}
