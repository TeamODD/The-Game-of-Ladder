// 카메라(또는 일반 오브젝트)를 시작 위치에서 끝 위치까지 일정한 속도로 이동시킨다
// End 씬에 진입하면 페이드 인과 동시에 스크롤 시작

using UnityEngine;

public class ScrollCredit : MonoBehaviour
{
    [SerializeField] private Vector2 startPosition = new Vector2(0f, 0f);
    [SerializeField] private Vector2 endPosition = new Vector2(0f, -20f);

    [Tooltip("초당 이동 거리 (월드 유닛/초)")]
    [SerializeField] private float speed = 2f;

    [Header("Fade In")]
    [Tooltip("페이드할 배경의 부모 오브젝트들")]
    [SerializeField] private Transform[] backgroundRoots;

    [Tooltip("페이드 인 시작 전 검은 화면을 유지하는 시간 (초)")]
    [SerializeField] private float blackHoldTime = 0.5f;

    [Tooltip("검은색에서 원래대로 돌아오는 데 걸리는 시간 (초)")]
    [SerializeField] private float fadeDuration = 1.5f;

    private SpriteRenderer[] backgroundRenderers;
    private Color[] originalColors;
    private float fadeTimer;
    private bool fadeFinished;

    void Start()
    {
        SetPosition(startPosition);

        CollectBackgroundRenderers();
        ApplyFade(0f); // 처음엔 배경을 완전히 검게
    }

    void Update()
    {
        if (!fadeFinished)
        {
            UpdateFade();

            // 검은 화면 유지 시간 동안은 멈춰 있다가, 페이드 인이 시작되면 스크롤도 함께 시작
            if (fadeTimer < blackHoldTime) return;
        }

        Vector2 current = transform.position;
        if (current == endPosition) return;

        // MoveTowards: 가감속 없이 매 프레임 같은 속도로 이동
        SetPosition(Vector2.MoveTowards(current, endPosition, speed * Time.deltaTime));
    }

    private void UpdateFade()
    {
        fadeTimer += Time.deltaTime;

        float t = fadeDuration > 0f
            ? Mathf.Clamp01((fadeTimer - blackHoldTime) / fadeDuration)
            : (fadeTimer >= blackHoldTime ? 1f : 0f);

        ApplyFade(t);

        if (t >= 1f)
        {
            fadeFinished = true;
        }
    }

    private void CollectBackgroundRenderers()
    {
        if (backgroundRoots != null && backgroundRoots.Length > 0)
        {
            var list = new System.Collections.Generic.List<SpriteRenderer>();
            foreach (Transform root in backgroundRoots)
            {
                if (root == null) continue;
                list.AddRange(root.GetComponentsInChildren<SpriteRenderer>(true));
            }
            backgroundRenderers = list.ToArray();
        }
        else
        {
            backgroundRenderers = FindObjectsByType<SpriteRenderer>();
        }

        originalColors = new Color[backgroundRenderers.Length];
        for (int i = 0; i < backgroundRenderers.Length; i++)
        {
            originalColors[i] = backgroundRenderers[i].color;
        }
    }

    // t = 0: 검은색, t = 1: 원래 색
    // 알파는 그대로 두고 RGB만 어둡게 하므로, 겹친 레이어가 비쳐 보이지 않는다.
    private void ApplyFade(float t)
    {
        for (int i = 0; i < backgroundRenderers.Length; i++)
        {
            if (backgroundRenderers[i] == null) continue;

            Color original = originalColors[i];
            backgroundRenderers[i].color = new Color(
                original.r * t,
                original.g * t,
                original.b * t,
                original.a);
        }
    }

    private void SetPosition(Vector2 position)
    {
        // z값(카메라의 경우 -10)은 그대로 유지
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }
}
