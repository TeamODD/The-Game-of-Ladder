// 모니터 화면비에 맞춰 카메라 크기를 조정하는 코드
// 16:9를 기준으로 게임을 만들고, 16:10, 4:3, 21:9 같은 다른 화면비에서도
// 검은 여백(레터박스) 없이 화면이 꽉 차도록 Orthographic Size를 자동으로 바꿔 준다.

/*
<내가 보려고 쓰는 보충 설명>
메인게임 씬에 Cinemachine - Targeted Camera - Follow Camera 추가

Position Composer
- Follow -> Position Composer로 변경
- Camera Distance: 10으로 설정
- Damping: 부드럽게 따라가는 정도(수치가 클 수록 카메라 이동이 느려짐)
- Dead Zone: 플레이어가 특정 영역 안에서 움직이면 카메라가 멈춰있음
- Lookhead: 이동 방향 앞쪽을 미리 보여줌    

Cinemachine Confiner 2D
- 맵 밖에 화면에 보이지 않게 함
- 방법: 맵 크기와 동일한 PolygonCollider2D를 만들어 Bounding Shape 2D에 넣기
*/

using UnityEngine;
using Unity.Cinemachine;

public class CameraResolution : MonoBehaviour
{
    public enum FitMode
    {
        Expand,  // 기준(16:9) 영역이 항상 전부 보이게 하고, 남는 부분은 주변 맵을 더 보여줌
        Crop     // 화면을 꽉 채우되, 기준 영역 중 화면 밖으로 나가는 부분은 잘라냄
    }

    public Vector2 referenceResolution = new Vector2(16f, 9f);  // 기준 화면비 (가로, 세로)
    public FitMode fitMode = FitMode.Expand;

    CinemachineCamera cmCamera;  // Cinemachine을 쓰는 경우
    Camera cam;                  // 일반 카메라를 쓰는 경우
    float baseOrthographicSize;  // 16:9 화면에서 쓸 원래 카메라 크기 (Inspector에 설정한 값)
    int lastScreenWidth;
    int lastScreenHeight;


    private void Awake()
    {
        cmCamera = GetComponent<CinemachineCamera>();

        if (cmCamera != null)
        {
            // Cinemachine: CinemachineBrain이 이 Lens 값을 Main Camera에 복사하므로 여기를 바꿔야 함
            baseOrthographicSize = cmCamera.Lens.OrthographicSize;
        }
        else
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                Debug.LogWarning("CameraResolution: Camera 또는 CinemachineCamera가 있는 오브젝트에 붙여 주세요.", this);
                enabled = false;
                return;
            }

            // 이 스크립트는 Orthographic 카메라 기준으로 계산하므로 Perspective면 경고
            if (!cam.orthographic)
            {
                Debug.LogWarning("CameraResolution: 카메라 Projection을 Orthographic으로 설정하세요.", this);
            }

            // 혹시 Viewport Rect가 바뀌어 있어도 화면 전체를 쓰게 함
            cam.rect = new Rect(0f, 0f, 1f, 1f);
            baseOrthographicSize = cam.orthographicSize;
        }

        ApplyResolution();
    }

    private void Update()
    {
        // 창 크기나 해상도가 바뀌었을 때만 다시 계산함
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            ApplyResolution();
        }
    }

    void ApplyResolution()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        if (Screen.height == 0 || referenceResolution.y <= 0f) return;  // 0으로 나누는 것 방지

        float targetAspect = referenceResolution.x / referenceResolution.y;  // 16:9 = 1.777...
        float screenAspect = (float)Screen.width / Screen.height;            // 현재 모니터 화면비

        // Orthographic Size는 화면 세로 길이의 절반을 뜻한다.
        // 가로로 보이는 범위 = Size * 2 * 화면비 이므로,
        // 화면비가 달라졌을 때 Size를 (기준 화면비 / 현재 화면비) 만큼 키우면 가로 범위가 기준과 같아진다.
        float widthMatchedSize = baseOrthographicSize * targetAspect / screenAspect;

        float newSize;
        if (fitMode == FitMode.Expand)
        {
            // 16:10, 4:3처럼 기준보다 좁은 화면: 가로를 맞추고 위아래를 더 보여줌
            // 21:9처럼 기준보다 넓은 화면: 세로를 유지하고 좌우를 더 보여줌
            newSize = Mathf.Max(baseOrthographicSize, widthMatchedSize);
        }
        else
        {
            // 좁은 화면: 세로를 유지하고 좌우를 잘라냄
            // 넓은 화면: 가로를 맞추고 위아래를 잘라냄
            newSize = Mathf.Min(baseOrthographicSize, widthMatchedSize);
        }

        if (cmCamera != null)
        {
            cmCamera.Lens.OrthographicSize = newSize;
        }
        else
        {
            cam.orthographicSize = newSize;
        }
    }
}
