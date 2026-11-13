// PlayerSlipState.cs
// 미끄러짐과 추락 상태 (플레이어 오브젝트에 넣기)
using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSlipState : MonoBehaviour
{
    [Header("미끄러짐 시작")]
    public float slipSpeed = 10f;  // 미끄러짐 시작 시 초기 속도

    [Header("중력 설정")]
    public float initialSlipGravity = 0.5f; // 튕김 직후 중력
    public float gravityIncrease = 0.1f;
    public float maxFallingGravity = 2.5f;       // 이후 강해지는 중력

    [Header("관성")]
    public float horizontalDeceleration = 0.5f;  // 초당 수평 속도 감소량
    public float counterControl = 1f;  // 반대 방향 입력 시 추가 감소량

    [Header("착지 판정")]
    public LayerMask groundLayer;
    public Vector2 groundCheckOffset = new Vector2(0f, -0.5f);  // 발 위치 (콜라이더에 맞게 조정)
    public Vector2 groundCheckSize = new Vector2(0.4f, 0.05f);
    public float minSlipTime = 0.2f;  // 시작 직후 바로 착지 처리되는 것 방지

    [Header("테스트 (에디터 전용)")]
    public bool enableTestKey = false;          // 켜면 T키로 미끄러짐 테스트
    public Vector2 testSlipDirection = new Vector2(1f, -0.3f);

    public bool IsSlipping = false;
    public event Action OnSlipStarted;
    public event Action OnSlipEnded;

    private Rigidbody2D rb;
    private Player player;
    private PlayerInteraction interaction;
    private float originalGravityScale;
    private float slipTimer;
    private float horizontalInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
        interaction = GetComponent<PlayerInteraction>();
        originalGravityScale = rb.gravityScale;
    }

    // 현재 진행 비율(0~1)이 미끄러짐 지점을 넘었는지 (등반 코드에서 사용)
    public static bool ShouldSlip(float climbProgress, SlipResult result)
    {

        return !result.isStable && climbProgress >= result.slipPoint;

    }

    // 미끄러짐 시작 (direction: 미끄러지는 방향)
    public void StartSlip(Vector2 direction)
    {
        if (IsSlipping)
            return;

        IsSlipping = true;
        slipTimer = 0f;
        horizontalInput = 0f;

        if (player != null)
        {
            player.ForceExitClimbing();
        }

        SetControlComponents(false);

        rb.gravityScale = initialSlipGravity ; // 등반 중 0이었던 중력 복구
        rb.linearVelocity = direction.normalized * slipSpeed;

        OnSlipStarted?.Invoke();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

/*
//#if UNITY_EDITOR 
        if (enableTestKey && !IsSlipping && Keyboard.current.tKey.wasPressedThisFrame)
            StartSlip(testSlipDirection);
//#endif 
*/

        if (!IsSlipping)
            return;

        // A/D만 읽음. W는 무시 (조작 불가)
        horizontalInput = 0f;
        if (Keyboard.current.aKey.isPressed) horizontalInput = -1f;
        if (Keyboard.current.dKey.isPressed) horizontalInput = 1f;
    }

    private void FixedUpdate()
    {
        if (!IsSlipping)
            return;

        slipTimer += Time.fixedDeltaTime;
        if (rb.gravityScale < maxFallingGravity)
        {
            rb.gravityScale += gravityIncrease;
        }


        Vector2 v = rb.linearVelocity;

        // 기본 관성 감소 + 추락 방향과 반대로 입력하면 조금 더 감소
        float decel = horizontalDeceleration;
        bool isCounterInput = horizontalInput != 0f && v.x != 0f
                              && Mathf.Sign(horizontalInput) != Mathf.Sign(v.x);
        if (isCounterInput)
            decel += counterControl;

        v.x = Mathf.MoveTowards(v.x, 0f, decel * Time.fixedDeltaTime);
        rb.linearVelocity = v;

        // 착지 판정 (하강 중일 때만)
        if (slipTimer >= minSlipTime && v.y <= 0f && IsGrounded())
        {
            EndSlip();
            return;
        }

    }

    private bool IsGrounded()
    {
        Vector2 center = (Vector2)transform.position + groundCheckOffset;
        return Physics2D.OverlapBox(center, groundCheckSize, 0f, groundLayer) != null;
    }

    private void EndSlip()
    {
        if (!IsSlipping)
            return;

        IsSlipping = false;
        rb.linearVelocity = Vector2.zero;

        SetControlComponents(true);

        OnSlipEnded?.Invoke();

    }

    // 미끄러짐 중에는 기존 이동/상호작용 스크립트를 멈춰 속도 덮어쓰기와 W 입력을 차단
    private void SetControlComponents(bool enabled)
    {
        if (player != null) player.enabled = enabled;
        if (interaction != null) interaction.enabled = enabled;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube((Vector2)transform.position + groundCheckOffset, groundCheckSize);
    }

}
