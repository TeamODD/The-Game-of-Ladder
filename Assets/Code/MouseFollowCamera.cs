using UnityEngine;
using UnityEngine.InputSystem;

// 타이틀 화면용: 카메라가 마우스 커서 방향으로 "살짝" 따라가도록 한다.
// 카메라가 움직이면 ParallaxBackground가 그 이동량을 받아 레이어별로 다르게 움직인다.
public class MouseFollowCamera : MonoBehaviour
{
    [Tooltip("마우스가 화면 끝에 있을 때 카메라가 시작 위치에서 벗어나는 최대 거리 (월드 단위). 작을수록 덜 움직임")]
    public Vector2 maxOffset = new Vector2(0.5f, 0.3f);

    [Tooltip("목표 위치까지 따라가는 데 걸리는 대략적인 시간(초). 클수록 느리고 부드럽게 따라감")]
    [Min(0.01f)]
    public float smoothTime = 0.6f;

    private Vector3 startPosition;
    private Vector3 velocity;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        // 마우스 위치를 화면 중앙 기준 -1 ~ 1 범위로 변환
        Vector2 mouse = Mouse.current.position.ReadValue();
        Vector2 normalized = new Vector2(
            mouse.x / Screen.width * 2f - 1f,
            mouse.y / Screen.height * 2f - 1f
        );
        // 커서가 창 밖으로 나가도 범위를 넘지 않게 제한
        normalized.x = Mathf.Clamp(normalized.x, -1f, 1f);
        normalized.y = Mathf.Clamp(normalized.y, -1f, 1f);

        Vector3 target = startPosition + new Vector3(
            normalized.x * maxOffset.x,
            normalized.y * maxOffset.y,
            0f
        );

        // 즉시 이동하지 않고 천천히 따라감
        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, smoothTime);
    }
}
