// EndingSequence.cs
// 엔딩 연출: Upper005 출력과 동시에 플레이어로 카메라 줌 인
// -> Upper006 출력이 끝나면 화면을 하얗게 페이드 아웃 -> SceneChanger.GoEnd()로 End 씬 이동
// 씬에 직접 붙이지 않아도 NarrationManager가 필요할 때 자동으로 추가한다.
// (직접 붙이면 Inspector에서 값 조절 가능)

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;

public class EndingSequence : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Transform zoomTarget;      // 비워두면 Player 오브젝트
    [SerializeField] private SceneChanger sceneChanger;

    [Header("줌 인")]
    [Tooltip("줌 인 후 카메라 크기 (Orthographic Size). 작을수록 더 확대됨")]
    [SerializeField] private float zoomedOrthoSize = 5f;
    [Tooltip("줌 인에 걸리는 시간(초)")]
    [SerializeField] private float zoomDuration = 0.8f;

    [Header("페이드 아웃")]
    [SerializeField] private Color fadeColor = Color.white;
    [Tooltip("하얗게 페이드 아웃되는 데 걸리는 시간(초)")]
    [SerializeField] private float fadeDuration = 1.5f;

    private bool isPlaying;
    private BGMManager bgmManager;

    private void Awake()
    {
        bgmManager = FindAnyObjectByType<BGMManager>();
    }

    // narrations를 각각 secondsEach초씩 출력하면서 엔딩 연출을 진행한다.
    public void Play(NarrationManager narrationManager, float secondsEach, params NarrationData[] narrations)
    {
        if (isPlaying) return;
        isPlaying = true;

        // 자막 출력과 줌 인을 동시에 시작
        narrationManager.ShowNarrationSequence(secondsEach, narrations);
        StartCoroutine(ZoomInRoutine());
        StartCoroutine(EndingRoutine(narrationManager));
        bgmManager.StopAllBGM(10f);
    }

    private IEnumerator EndingRoutine(NarrationManager narrationManager)
    {
        // 마지막 자막이 끝나고 자막 패널이 사라질 때까지 대기
        // (표시 시간을 NarrationManager에서 바꿔도 자동으로 맞춰짐)
        yield return null;
        yield return new WaitUntil(() => !narrationManager.narrationPanel.activeSelf);

        yield return FadeOutRoutine();

        if (sceneChanger == null)
            sceneChanger = FindAnyObjectByType<SceneChanger>();

        // InGame 씬에 SceneChanger가 없으면 임시로 만들어서 사용
        if (sceneChanger == null)
            sceneChanger = new GameObject("SceneChanger (Ending)").AddComponent<SceneChanger>();

        sceneChanger.GoEnd();
    }

    private IEnumerator ZoomInRoutine()
    {
        if (cinemachineCamera == null)
            cinemachineCamera = FindAnyObjectByType<CinemachineCamera>();
        if (cinemachineCamera == null)
        {
            Debug.LogWarning("EndingSequence: CinemachineCamera를 찾지 못해 줌 인을 건너뜁니다.");
            yield break;
        }

        if (zoomTarget == null)
        {
            Player player = FindAnyObjectByType<Player>();
            if (player != null)
                zoomTarget = player.transform;
        }

        // 화면 크기 변경 시 CameraResolution이 카메라 크기를 되돌리지 않도록 끔
        CameraResolution resolution = cinemachineCamera.GetComponent<CameraResolution>();
        if (resolution != null)
            resolution.enabled = false;

        Camera outputCamera = Camera.main;
        Transform camTransform = cinemachineCamera.transform;

        // Confiner2D는 켜 둔 채로, 경계 모양(MapBoundary)만 가져와서 직접 계산에 사용
        CinemachineConfiner2D confiner = cinemachineCamera.GetComponent<CinemachineConfiner2D>();
        Collider2D boundary = confiner != null ? confiner.BoundingShape2D : null;

        // 실제 화면에 보이는 위치(경계 보정이 끝난 위치)에서 출발해야 시작할 때 화면이 튀지 않음
        Vector2 startPos = outputCamera != null ? (Vector2)outputCamera.transform.position : (Vector2)camTransform.position;

        // Follow를 끊고 줌 동안 카메라 위치를 직접 제어
        // (Cinemachine이 플레이어를 화면 중앙에 두려고 경계와 다투지 않게 하기 위함)
        cinemachineCamera.Follow = null;
        camTransform.position = new Vector3(startPos.x, startPos.y, camTransform.position.z);

        // 우리가 계산한 위치는 항상 경계 안이므로 Confiner의 감속/지연은 필요 없음 (화면 출렁임 방지)
        if (confiner != null)
        {
            confiner.Damping = 0f;
            confiner.SlowingDistance = 0f;
        }

        float startSize = cinemachineCamera.Lens.OrthographicSize;
        float aspect = outputCamera != null ? outputCamera.aspect : 16f / 9f;
        float elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / zoomDuration));
            float size = Mathf.Lerp(startSize, zoomedOrthoSize, t);

            // 목표: 플레이어를 화면 중앙에 두되, 현재 크기에서 맵 밖이 보이지 않는 범위로 제한한 위치
            Vector2 targetPos = zoomTarget != null
                ? ClampToBoundary(zoomTarget.position, size, aspect, boundary)
                : startPos;

            // 시작 위치 -> 목표 위치로 이동하면서 크기도 함께 줄임
            // (두 위치 모두 경계 안이므로 그 사이의 모든 위치도 경계 안 -> 맵 밖이 보이지 않음)
            Vector2 pos = Vector2.Lerp(startPos, targetPos, t);

            cinemachineCamera.Lens.OrthographicSize = size;
            camTransform.position = new Vector3(pos.x, pos.y, camTransform.position.z);
            yield return null;
        }

        cinemachineCamera.Lens.OrthographicSize = zoomedOrthoSize;

        // 줌 인이 끝난 뒤에도 End 씬으로 넘어갈 때까지 계속 플레이어를 따라감
        // (줌 마지막 프레임과 같은 계산이라 움직임이 끊기지 않음)
        if (zoomTarget == null) yield break;

        while (true)
        {
            Vector2 pos = ClampToBoundary(zoomTarget.position, zoomedOrthoSize, aspect, boundary);
            camTransform.position = new Vector3(pos.x, pos.y, camTransform.position.z);
            yield return null;
        }
    }

    // 카메라 중심을 point에 두고 싶을 때, 화면(orthoSize 크기)이 경계 밖을 보여주지 않도록 제한한 위치를 반환
    private static Vector2 ClampToBoundary(Vector2 point, float orthoSize, float aspect, Collider2D boundary)
    {
        if (boundary == null) return point;

        Bounds b = boundary.bounds;
        float halfHeight = orthoSize;
        float halfWidth = orthoSize * aspect;

        // 화면이 경계보다 크면 경계 중앙에 고정
        float x = halfWidth * 2f >= b.size.x
            ? b.center.x
            : Mathf.Clamp(point.x, b.min.x + halfWidth, b.max.x - halfWidth);
        float y = halfHeight * 2f >= b.size.y
            ? b.center.y
            : Mathf.Clamp(point.y, b.min.y + halfHeight, b.max.y - halfHeight);

        return new Vector2(x, y);
    }

    private IEnumerator FadeOutRoutine()
    {
        Image fadeImage = CreateFadeImage();

        Color color = fadeColor;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
    }

    // 화면 전체를 덮는 페이드용 Image를 코드로 생성 (다른 UI보다 위에 그려짐)
    private Image CreateFadeImage()
    {
        GameObject canvasObject = new GameObject("EndingFadeCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        GameObject imageObject = new GameObject("FadeImage");
        imageObject.transform.SetParent(canvasObject.transform, false);

        Image image = imageObject.AddComponent<Image>();
        Color startColor = fadeColor;
        startColor.a = 0f;
        image.color = startColor;
        image.raycastTarget = true; // 페이드 중 클릭 막기

        RectTransform rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return image;
    }
}
