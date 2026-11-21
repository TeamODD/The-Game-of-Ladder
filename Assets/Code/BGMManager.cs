
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    private AudioSource audioSource;
    private AudioSource additionalAudioSource;

    private Coroutine fadeCoroutine;
    private Coroutine additionalCoroutine;

    private Player player;
    private AudioClip requestedClip;

    [Header("메인 배경음악")]
    public AudioClip lower;
    public AudioClip middle;
    public AudioClip upper;

    [Header("메인 페이드 설정")]
    public float fadeDuration = 2f;
    public float maxVolume = 0.5f;

    [Header("추가 배경음악")]
    public AudioClip[] additionalTracks;
    public float additionalVolume = 0.3f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        player = FindAnyObjectByType<Player>();

        // 메인 AudioSource
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.volume = 0f;
        audioSource.spatialBlend = 0f;

        // 추가 AudioSource
        additionalAudioSource = gameObject.AddComponent<AudioSource>();

        additionalAudioSource.playOnAwake = false;
        additionalAudioSource.loop = false;
        additionalAudioSource.volume = additionalVolume;
        additionalAudioSource.spatialBlend = 0f;
    }

    private void Start()
    {
        PlayBGM(lower);
        StartAdditionalBGM();
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

    // 메인 배경음악 변경
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || requestedClip == clip)
            return;

        requestedClip = clip;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(ChangeBGM(clip));
    }

    // 메인 배경음악 페이드 처리
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

    // 추가 배경음악 시작
    public void StartAdditionalBGM()
    {
        if (additionalTracks == null || additionalTracks.Length == 0)
            return;

        if (additionalCoroutine != null)
            StopCoroutine(additionalCoroutine);

        additionalCoroutine = StartCoroutine(PlayAdditionalTracks());
    }

    // 추가 배경음악 셔플 및 반복 재생
    private IEnumerator PlayAdditionalTracks()
    {
        List<AudioClip> shuffledTracks = new List<AudioClip>();

        foreach (AudioClip clip in additionalTracks)
        {
            if (clip != null)
                shuffledTracks.Add(clip);
        }

        if (shuffledTracks.Count == 0)
            yield break;

        AudioClip previousClip = null;

        while (true)
        {
            // 음악 목록 섞기
            for (int i = shuffledTracks.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);

                AudioClip temp = shuffledTracks[i];
                shuffledTracks[i] = shuffledTracks[randomIndex];
                shuffledTracks[randomIndex] = temp;
            }

            // 이전 재생곡과 첫 곡이 같으면 순서 변경
            if (shuffledTracks.Count > 1 &&
                shuffledTracks[0] == previousClip)
            {
                AudioClip temp = shuffledTracks[0];
                shuffledTracks[0] = shuffledTracks[1];
                shuffledTracks[1] = temp;
            }

            // 섞인 순서대로 재생
            foreach (AudioClip clip in shuffledTracks)
            {
                additionalAudioSource.clip = clip;
                additionalAudioSource.volume = additionalVolume;
                additionalAudioSource.Play();

                yield return new WaitForSeconds(clip.length);

                previousClip = clip;
            }
        }
    }

    // 추가 배경음악 정지
    public void StopAdditionalBGM()
    {
        if (additionalCoroutine != null)
        {
            StopCoroutine(additionalCoroutine);
            additionalCoroutine = null;
        }

        additionalAudioSource.Stop();
    }

    // 추가 배경음악 볼륨 변경
    public void SetAdditionalVolume(float volume)
    {
        additionalVolume = Mathf.Clamp01(volume);
        additionalAudioSource.volume = additionalVolume;
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
