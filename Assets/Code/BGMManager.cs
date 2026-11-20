
using System.Collections;
using UnityEngine;

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
        // 싱글톤 중복 생성 방지
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

    // 배경음악 변경 요청
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || requestedClip == clip)
            return;

        requestedClip = clip;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(ChangeBGM(clip));
    }

    // 배경음악 페이드 아웃 및 페이드 인
    private IEnumerator ChangeBGM(AudioClip newClip)
    {
        yield return FadeVolume(audioSource.volume, 0f);

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.volume = 0f;
        audioSource.Play();

        yield return FadeVolume(0f, maxVolume);

        audioSource.volume = maxVolume;
        fadeCoroutine = null;
    }

    // 볼륨 페이드 처리
    private IEnumerator FadeVolume(float startVolume, float targetVolume)
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            audioSource.volume = Mathf.Lerp(
                startVolume,
                targetVolume,
                timer / fadeDuration
            );

            yield return null;
        }

        audioSource.volume = targetVolume;
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
