using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Background Music")]
    public AudioSource bgmSource;

    [Header("Sound Effects")]
    public AudioSource sfxSource;

    [Header("Background Music Clips")]
    public AudioClip bgmMainGameplay;
    public AudioClip bgmVictory;

    [Header("Sound Effect Clips")]
    public AudioClip footstep;
    public AudioClip cutShadow;
    public AudioClip pasteShadow;
    public AudioClip revealShadow;
    public AudioClip lantern;
    public AudioClip key;
    public AudioClip doorOpen;
    public AudioClip pressurePlate;
    public AudioClip button;
    public AudioClip shadowSeeker;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 游戏开始自动播放主BGM
            PlayBGM(bgmMainGameplay);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(clip);
    }
}