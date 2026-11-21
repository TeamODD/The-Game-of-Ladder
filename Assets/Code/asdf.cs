
using System.Collections;
using UnityEngine;

public class FunObjectEffect : MonoBehaviour
{
    [Header("효과음")]
    public AudioClip soundEffect;

    [Range(0f, 1f)]
    public float soundVolume = 0.5f;

    [Header("페이드 인")]
    public float fadeInDuration = 1f;

    [Header("위치 설정")]
    public Vector3 headOffset = new Vector3(0f, 1.5f, 0f);

    [Header("크기 변화")]
    public float scaleAmount = 0.15f;
    public float scaleSpeed = 3f;

    private AudioSource audioSource;
    private Transform player;

    private Vector3 originalScale;
    private bool activated = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        originalScale = transform.localScale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated || !other.CompareTag("Player"))
            return;

        activated = true;
        player = other.transform;

        // 효과음 페이드 인
        if (soundEffect != null)
            StartCoroutine(PlaySoundWithFade());

        // 플레이어 머리 위로 이동
        transform.SetParent(player);
        transform.localPosition = headOffset;
        transform.localRotation = Quaternion.identity;

        // 충돌 비활성화
        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
            col.enabled = false;
    }

    private IEnumerator PlaySoundWithFade()
    {
        audioSource.clip = soundEffect;
        audioSource.volume = 0f;
        audioSource.Play();

        float timer = 0f;

        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;

            audioSource.volume = Mathf.Lerp(
                0f,
                soundVolume,
                timer / fadeInDuration
            );

            yield return null;
        }

        audioSource.volume = soundVolume;
    }

    private void Update()
    {
        if (!activated)
            return;

        // 크기 변화 반복
        float scale = 1f + Mathf.Sin(Time.time * scaleSpeed) * scaleAmount;

        transform.localScale = originalScale * scale;
    }
}
