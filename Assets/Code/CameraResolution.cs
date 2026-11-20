
using UnityEngine;
using Unity.Cinemachine;

public class CameraResolution : MonoBehaviour
{
    public enum FitMode
    {
        Expand, // 기준 영역을 전부 보여주고 남는 영역 확장
        Crop    // 화면을 채우고 기준 영역 일부를 잘라냄
    }

    [Header("화면 설정")]
    [Tooltip("기준 화면비 (기본값 16:9)")]
    public Vector2 referenceResolution = new Vector2(16f, 9f);

    [Tooltip("화면비에 따른 카메라 조정 방식")]
    public FitMode fitMode = FitMode.Expand;


    private CinemachineCamera cmCamera;
    private Camera cam;

    private float baseOrthographicSize;

    private int lastScreenWidth;
    private int lastScreenHeight;


    // =========================
    // 초기화
    // =========================

    private void Awake()
    {
        if (!InitializeCamera())
        {
            enabled = false;
            return;
        }

        ApplyResolution();
    }


    private bool InitializeCamera()
    {
        // Cinemachine 카메라 확인
        cmCamera = GetComponent<CinemachineCamera>();

        if (cmCamera != null)
        {
            baseOrthographicSize =
                cmCamera.Lens.OrthographicSize;

            return true;
        }

        // 일반 카메라 확인
        cam = GetComponent<Camera>();

        if (cam == null)
        {
            Debug.LogWarning(
                "CameraResolution: Camera 또는 CinemachineCamera가 필요합니다.",
                this
            );

            return false;
        }

        // Orthographic 카메라 확인
        if (!cam.orthographic)
        {
            Debug.LogWarning(
                "CameraResolution: Projection을 Orthographic으로 설정하세요.",
                this
            );

            return false;
        }

        // 전체 화면 사용
        cam.rect = new Rect(0f, 0f, 1f, 1f);

        baseOrthographicSize = cam.orthographicSize;

        return true;
    }


    // =========================
    // 화면 크기 확인
    // =========================

    private void Update()
    {
        if (Screen.width == lastScreenWidth &&
            Screen.height == lastScreenHeight)
        {
            return;
        }

        ApplyResolution();
    }


    // =========================
    // 해상도 적용
    // =========================

    private void ApplyResolution()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        if (Screen.height <= 0 ||
            referenceResolution.x <= 0f ||
            referenceResolution.y <= 0f)
        {
            return;
        }

        float targetAspect =
            referenceResolution.x / referenceResolution.y;

        float screenAspect =
            (float)Screen.width / Screen.height;

        float newSize = CalculateOrthographicSize(
            targetAspect,
            screenAspect
        );

        SetCameraSize(newSize);
    }


    // =========================
    // 카메라 크기 계산
    // =========================

    private float CalculateOrthographicSize(
        float targetAspect,
        float screenAspect)
    {
        // 가로 영역을 기준으로 계산한 Orthographic Size
        float widthMatchedSize =
            baseOrthographicSize * targetAspect / screenAspect;

        switch (fitMode)
        {
            case FitMode.Expand:
                return Mathf.Max(
                    baseOrthographicSize,
                    widthMatchedSize
                );

            case FitMode.Crop:
                return Mathf.Min(
                    baseOrthographicSize,
                    widthMatchedSize
                );

            default:
                return baseOrthographicSize;
        }
    }


    // =========================
    // 카메라 크기 적용
    // =========================

    private void SetCameraSize(float size)
    {
        if (cmCamera != null)
        {
            cmCamera.Lens.OrthographicSize = size;
        }
        else if (cam != null)
        {
            cam.orthographicSize = size;
        }
    }
}
