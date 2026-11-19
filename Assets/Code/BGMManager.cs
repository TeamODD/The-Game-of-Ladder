
using UnityEngine;
using System.Collections;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;
    private Player player;
    private AudioClip requestedClip;

    [Header("배경음악")]
    public AudioClip lower;
    public AudioClip middle;
    public AudioClip upper;

    [Header("페이드 설정")]
    public float fadeDuration = 2f;
    public float maxVolume = 0.5f;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        player = FindAnyObjectByType<Player>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.volume = 0f;
        audioSource.spatialBlend = 0f;
    }

    private void Start()
    {
        PlayBGM(lower);
    }

    private void Update()
    {
        if (player == null)
            return;

        if (player.isLower)
            PlayBGM(lower);
        else if (player.isMiddle)
            PlayBGM(middle);
        else if (player.isUpper)
            PlayBGM(upper);
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || requestedClip == clip)
            return;

        requestedClip = clip;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(ChangeBGM(clip));
    }
    private IEnumerator ChangeBGM(AudioClip newClip)
    {
        // 기존 음악 페이드 아웃
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            audioSource.volume = Mathf.Lerp(
                startVolume,
                0f,
                timer / fadeDuration
            );

            yield return null;
        }

        // 기존 음악 정지
        audioSource.Stop();

        // 새로운 음악 재생
        audioSource.clip = newClip;
        audioSource.volume = 0f;
        audioSource.Play();

        // 새 음악 페이드 인
        timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            audioSource.volume = Mathf.Lerp(
                0f,
                maxVolume,
                timer / fadeDuration
            );

            yield return null;
        }

        audioSource.volume = maxVolume;
        fadeCoroutine = null;
    }

    public void PlayLower()
    {
        PlayBGM(lower);
    }

    public void PlayMiddle()
    {
        PlayBGM(middle);
    }

    public void PlayUpper()
    {
        PlayBGM(upper);
    }
}
