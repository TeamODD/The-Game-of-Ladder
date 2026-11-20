
using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSlipState : MonoBehaviour
{
    [Header("미끄러짐 시작")]
    public float slipSpeed = 10f;

    [Header("상태별 미끄러짐 속도")]
    public float lowerSlipSpeed = 6f;
    public float middleSlipSpeed = 10f;
    public float upperSlipSpeed = 14f;

    [Header("상태별 초기 중력")]
    public float lowerSlipGravity = 0.5f;
    public float middleSlipGravity = 0.7f;
    public float upperSlipGravity = 1f;

    [Header("중력 설정")]
    public float initialSlipGravity = 0.5f;
    public float gravityIncrease = 0.1f;
    public float maxFallingGravity = 2.5f;

    [Header("튕겨 나가는 방향")]
    public float horizontalBounce = 1f;
    public float verticalBounce = 0.3f;

    [Header("관성")]
    public float horizontalDeceleration = 0.5f;
    public float counterControl = 1f;

    [Header("착지 판정")]
    public LayerMask groundLayer;
    public Vector2 groundCheckOffset = new Vector2(0f, -0.5f);
    public Vector2 groundCheckSize = new Vector2(0.4f, 0.05f);
    public float minSlipTime = 0.2f;

    [Header("테스트 (에디터 전용)")]
    public bool enableTestKey = false;
    public Vector2 testSlipDirection = new Vector2(1f, -0.3f);

    public bool IsSlipping = false;
    public event Action OnSlipStarted;
    public event Action OnSlipEnded;

    public NarrationManager narrationManager;

    private Rigidbody2D rb;
    private Player player;
    private PlayerInteraction interaction;
    private float slipTimer;
    private float horizontalInput;
    private SpriteRenderer playerSpriteRenderer;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
        interaction = GetComponent<PlayerInteraction>();
        playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    // 등반 진행률이 미끄러짐 지점을 넘었는지 확인
    public static bool ShouldSlip(float climbProgress, SlipResult result)
    {
        return !result.isStable && climbProgress >= result.slipPoint;
    }

    // 미끄러짐 시작
    public void StartSlip()
    {
        if (IsSlipping)
            return;

        IsSlipping = true;
        animator.SetBool("isSlip", true);
        narrationManager.ShowSlipNarration();

        slipTimer = 0f;
        horizontalInput = 0f;

        if (player != null)
            player.ForceExitClimbing();

        SetControlComponents(false);

        float horizontalDirection = playerSpriteRenderer.flipX ? 1f : -1f;

        Vector2 direction = new Vector2(
            horizontalBounce * horizontalDirection,
            -verticalBounce
        );

        direction.Normalize();

        float selectedSpeed = slipSpeed;
        float selectedGravity = initialSlipGravity;

        if (player.isLower)
        {
            selectedSpeed = lowerSlipSpeed;
            selectedGravity = lowerSlipGravity;
        }
        else if (player.isMiddle)
        {
            selectedSpeed = middleSlipSpeed;
            selectedGravity = middleSlipGravity;
        }
        else if (player.isUpper)
        {
            selectedSpeed = upperSlipSpeed;
            selectedGravity = upperSlipGravity;
        }

        rb.gravityScale = selectedGravity;
        rb.linearVelocity = direction.normalized * selectedSpeed;

        OnSlipStarted?.Invoke();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        /*
        //#if UNITY_EDITOR
        if (enableTestKey && !IsSlipping && Keyboard.current.tKey.wasPressedThisFrame)
            StartSlip();
        //#endif
        */

        if (!IsSlipping)
            return;

        horizontalInput = 0f;

        if (Keyboard.current.aKey.isPressed)
            horizontalInput = -1f;

        if (Keyboard.current.dKey.isPressed)
            horizontalInput = 1f;
    }

    private void FixedUpdate()
    {
        if (!IsSlipping)
            return;

        slipTimer += Time.fixedDeltaTime;

        if (rb.gravityScale < maxFallingGravity)
            rb.gravityScale += gravityIncrease;

        Vector2 velocity = rb.linearVelocity;

        float decel = horizontalDeceleration;

        bool isCounterInput =
            horizontalInput != 0f &&
            velocity.x != 0f &&
            Mathf.Sign(horizontalInput) != Mathf.Sign(velocity.x);

        if (isCounterInput)
            decel += counterControl;

        velocity.x = Mathf.MoveTowards(
            velocity.x,
            0f,
            decel * Time.fixedDeltaTime
        );

        rb.linearVelocity = velocity;

        // 하강 중일 때만 착지 판정
        if (slipTimer >= minSlipTime && velocity.y <= 0f && IsGrounded())
            EndSlip();
    }

    private bool IsGrounded()
    {
        Vector2 center = (Vector2)transform.position + groundCheckOffset;

        return Physics2D.OverlapBox(
            center,
            groundCheckSize,
            0f,
            groundLayer
        ) != null;
    }

    private void EndSlip()
    {
        if (!IsSlipping)
            return;

        Debug.Log("착지발동");

        IsSlipping = false;
        animator.SetBool("isSlip", false);

        player.ForceExitClimbing();
        rb.linearVelocity = Vector2.zero;

        SetControlComponents(true);

        OnSlipEnded?.Invoke();
    }

    // 미끄러지는 동안 이동 및 상호작용 스크립트 비활성화
    private void SetControlComponents(bool enabled)
    {
        if (player != null)
            player.enabled = enabled;

        if (interaction != null)
            interaction.enabled = enabled;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(
            (Vector2)transform.position + groundCheckOffset,
            groundCheckSize
        );
    }
}
