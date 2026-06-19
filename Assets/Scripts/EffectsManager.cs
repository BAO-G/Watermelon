using UnityEngine;
using DG.Tweening;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager Instance { get; private set; }

    [Header("合并特效预制体（可选）")]
    [SerializeField] private GameObject mergeParticlePrefab;

    [Header("屏幕震动")]
    [SerializeField] private bool enableCameraShake = true;
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeStrength = 0.1f;

    private Camera mainCamera;
    private Vector3 cameraOriginalPos;

    private void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
    }

    public void PlayMergeEffect(Vector2 position, int fruitLevel)
    {
        if (mergeParticlePrefab != null)
        {
            GameObject effect = Instantiate(mergeParticlePrefab, position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        if (enableCameraShake && mainCamera != null)
        {
            cameraOriginalPos = mainCamera.transform.position;
            mainCamera.transform.DOShakePosition(shakeDuration, shakeStrength)
                .OnComplete(() => mainCamera.transform.position = cameraOriginalPos);
        }
    }
}
