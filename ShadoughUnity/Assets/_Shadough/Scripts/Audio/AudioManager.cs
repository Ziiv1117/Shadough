using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;


    [Header("Background Music")]
    public AudioClip bgmMainGameplay;
    public AudioClip bgmVictory;


    [Header("Player SFX")]
    public AudioClip footstep;
    public AudioClip cutShadow;
    public AudioClip pasteShadow;
    public AudioClip revealShadow;
    public AudioClip lantern;


    [Header("Interaction SFX")]
    public AudioClip key;
    public AudioClip doorOpen;
    public AudioClip pressurePlate;
    public AudioClip button;


    [Header("Enemy SFX")]
    public AudioClip shadowSeeker;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        EnsureSources();
        PlayBGM(bgmMainGameplay);
    }

    private void Start()
    {
        EnsureSources();

        if (bgmSource != null
            && bgmMainGameplay != null
            && (!bgmSource.isPlaying || bgmSource.clip != bgmMainGameplay))
        {
            PlayBGM(bgmMainGameplay);
        }
    }


    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        EnsureSources();
        if (bgmSource == null) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }



    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        EnsureSources();
        if (sfxSource == null) return;

        sfxSource.PlayOneShot(clip);
    }

    private void EnsureSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
    }
}
