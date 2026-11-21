
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 5f;
    public float slowMultiplier = 0.5f;
    public bool canMove = true;

    public bool IsSlow => Keyboard.current != null &&
    Keyboard.current.leftShiftKey.isPressed ||
    Keyboard.current != null &&
    Keyboard.current.rightShiftKey.isPressed;

    public float SpeedMultiplier => IsSlow ? slowMultiplier : 1f;

    [Header("상태")]
    public bool isClimbing = false;
    public bool isUpper = false;
    public bool isMiddle = false;
    public bool isLower = false;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator animator;
    private PlayerSlipState slipState;

    private int moveInput;
    private float originalGravityScale;

    // 현재 등반 중인 사다리 정보
    private Transform currentLadderPath;
    private Transform currentLadderStart;
    private Transform currentLadderCheck;
    private Transform currentLadderEnd;
    private LadderSlipData currentLadderSlipData;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        slipState = GetComponent<PlayerSlipState>();

        originalGravityScale = rb.gravityScale;
        rb.freezeRotation = true;
    }


    private void Update()
    {
        HandleMovementInput();
    }


    private void FixedUpdate()
    {
        float currentSpeed = moveSpeed * SpeedMultiplier;

        if (isClimbing && currentLadderPath != null)
        {
            if (CheckLadderSlip())
                return;

            Vector2 pathDirection = currentLadderPath.up.normalized;

            bool slopesRight = pathDirection.x >= 0;
            int climbInput = slopesRight ? moveInput : -moveInput;

            rb.gravityScale = 0f;
            rb.linearVelocity = pathDirection * climbInput * currentSpeed;
        }
        else
        {
            rb.gravityScale = originalGravityScale;

            rb.linearVelocity = new Vector2(
                moveInput * currentSpeed,
                rb.linearVelocity.y
            );
        }
    }


    // =========================
    // 이동 입력
    // =========================

    private void HandleMovementInput()
    {
        if (!canMove)
        {
            moveInput = 0;
            animator.SetBool("isWalk", false);
            return;
        }

        if (Keyboard.current == null)
        {
            moveInput = 0;
            animator.SetBool("isWalk", false);
            return;
        }

        moveInput = 0;

        if (Keyboard.current.aKey.isPressed)
        {
            moveInput = -1;

            if (!isClimbing)
                sprite.flipX = true;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            moveInput = 1;

            if (!isClimbing)
                sprite.flipX = false;
        }

        animator.SetBool("isWalk", moveInput != 0);
    }


    // =========================
    // 층 구분
    // =========================

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Lower"))
        {
            isLower = true;
            isMiddle = false;
            isUpper = false;
        }
        else if (other.CompareTag("Middle"))
        {
            isLower = false;
            isMiddle = true;
            isUpper = false;
        }
        else if (other.CompareTag("Upper"))
        {
            isLower = false;
            isMiddle = false;
            isUpper = true;
        }
    }


    // =========================
    // 사다리 진입 및 하차
    // =========================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("LadderStart"))
        {
            if (isClimbing)
            {
                ExitLadder();
                return;
            }
            EnterLadder(other);
        }
        else if (other.CompareTag("LadderEnd"))
        {
            ExitLadder();
        }
    }


    private void EnterLadder(Collider2D other)
    {
        if (slipState != null && slipState.IsSlipping)
            return;

        if (isClimbing || moveInput == 0)
            return;

        Transform ladderRoot = other.transform.parent;

        if (ladderRoot == null)
            return;

        // 사다리 구성요소 참조
        currentLadderPath = ladderRoot.Find("LadderPath");
        currentLadderStart = ladderRoot.Find("LadderStart");
        currentLadderCheck = ladderRoot.Find("LadderCheck");
        currentLadderSlipData = ladderRoot.GetComponent<LadderSlipData>();

        // 독립된 LadderEnd 참조
        currentLadderEnd = currentLadderSlipData != null
            ? currentLadderSlipData.ladderEnd
            : null;

        // 필수 구성요소 확인
        if (currentLadderPath == null ||
            currentLadderCheck == null ||
            currentLadderSlipData == null)
        {
            Debug.LogWarning("사다리 구성요소를 찾지 못했습니다.");
            ClearLadderReferences();
            return;
        }

        // 사다리 진입 방향 확인
        Vector2 pathDirection = currentLadderPath.up.normalized;

        bool slopesRight = pathDirection.x >= 0;

        float playerSide = transform.position.x - other.transform.position.x;

        bool canEnter =
            (slopesRight && playerSide < 0) ||
            (!slopesRight && playerSide > 0);

        if (!canEnter)
        {
            ClearLadderReferences();
            return;
        }

        // 등반 시작
        isClimbing = true;
        animator.SetBool("isClimb", true);

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        transform.position = currentLadderPath.position;
    }


    private void ExitLadder()
    {
        if (!isClimbing)
            return;

        isClimbing = false;

        animator.SetBool("isClimb", false);

        rb.gravityScale = originalGravityScale;
        rb.linearVelocity = Vector2.zero;

        ClearLadderReferences();
    }


    private void ClearLadderReferences()
    {
        currentLadderPath = null;
        currentLadderStart = null;
        currentLadderCheck = null;
        currentLadderEnd = null;
        currentLadderSlipData = null;
    }


    // =========================
    // 강제 하차
    // =========================

    public void ForceExitClimbing()
    {
        if (!isClimbing)
            return;

        isClimbing = false;

        animator.SetBool("isClimb", false);

        rb.gravityScale = originalGravityScale;

        ClearLadderReferences();

        canMove = true;
    }


    // =========================
    // 미끄러짐 판정
    // =========================

    private bool CheckLadderSlip()
    {
        // 미끄러짐 판정에 필요한 정보 확인
        if (slipState == null ||
            currentLadderSlipData == null ||
            !currentLadderSlipData.HasResult ||
            currentLadderStart == null ||
            currentLadderCheck == null)
        {
            return false;
        }

        SlipResult result = currentLadderSlipData.Result;

        // 안정적인 사다리는 미끄러지지 않음
        if (result.isStable)
            return false;

        // LadderStart와 LadderCheck를 기준으로 진행도 계산
        Vector2 start = currentLadderStart.position;
        Vector2 end = currentLadderCheck.position;

        Vector2 ladderDirection = end - start;

        if (ladderDirection.sqrMagnitude < 0.0001f)
            return false;

        float climbProgress = Vector2.Dot(
            (Vector2)transform.position - start,
            ladderDirection
        ) / ladderDirection.sqrMagnitude;

        climbProgress = Mathf.Clamp01(climbProgress);

        // 미끄러짐 판정
        if (PlayerSlipState.ShouldSlip(climbProgress, result))
        {
            slipState.StartSlip();
            return true;
        }

        return false;
    }
}
