using UnityEngine;

public enum SoundType
{
    Drop,
    Merge,
    GameOver,
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("音效剪辑（可选，未赋值则静默）")]
    [SerializeField] private AudioClip dropClip;
    [SerializeField] private AudioClip mergeClip;
    [SerializeField] private AudioClip gameOverClip;

    [Header("音量")]
    [SerializeField] [Range(0f, 1f)] private float masterVolume = 0.8f;

    private AudioSource audioSource;

    private void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public void PlayDropSound()
    {
        Play(dropClip);
    }

    public void PlayMergeSound()
    {
        Play(mergeClip);
    }

    public void PlayGameOverSound()
    {
        Play(gameOverClip);
    }

    private void Play(AudioClip clip)
    {
        if (clip == null || audioSource == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, masterVolume);
    }
}
