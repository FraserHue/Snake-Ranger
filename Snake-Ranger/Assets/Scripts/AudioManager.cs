using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("------------ Audio Source ------------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("------------ Audio Clip ------------")]
    public AudioClip background; 
    public AudioClip death;
    public AudioClip snake_damaged;
    public AudioClip win;
    public AudioClip spider_damaged;
    public AudioClip uiHover;
    public AudioClip uiClick;
    public AudioClip playClick;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); 

        if (SFXSource == null)
            SFXSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        if (musicSource != null && background != null)
        {
            musicSource.clip = background;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (SFXSource == null || clip == null) return;
        SFXSource.PlayOneShot(clip);
    }
}
