using System.IO;
using Unity.VisualScripting.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;

public class Player : MonoBehaviour
{
    public float moveSpeed;
    public bool canMove = true;
    public bool isClimbing = false;

    private Rigidbody2D rb;
    private int moveInput;
    private SpriteRenderer sprite;
    private float originalGravityScale;


    private Animator animator;

    private Transform LadderPath;
    private Transform currentLadderPath;

    private PlayerSlipState slipState;

    private Transform currentLadderStart;
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

    private void Start()
    {
        moveSpeed = 5f;
    }

    private void Update()
    {
        if (!canMove)
        {
            moveInput = 0;
            return;
        }

        moveInput = 0;

        if (Keyboard.current.aKey.isPressed)
            moveInput = -1;

        if (Keyboard.current.dKey.isPressed)
            moveInput = 1;
    }

    private void FixedUpdate()
    {
        if (isClimbing && currentLadderPath != null)
        {
            if (CheckLadderSlip())
                return;

            Vector2 pathDirection = currentLadderPath.up.normalized;

            bool slopesRight = pathDirection.x >= 0;

            int climbInput = slopesRight ? moveInput : -moveInput;

            rb.gravityScale = 0f;

            rb.linearVelocity =
                pathDirection * climbInput * moveSpeed;
            
        }
        else
        {
            rb.gravityScale = originalGravityScale;

            rb.linearVelocity = new Vector2(
                moveInput * moveSpeed,
                rb.linearVelocity.y
            );
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        // LadderStart에 진입하면 사다리 탑승
        if (other.CompareTag("LadderStart"))
        {
            if (isClimbing || moveInput == 0)
            {
                isClimbing = false;
                currentLadderPath = null;

                rb.gravityScale = originalGravityScale;
                rb.linearVelocity = Vector2.zero;
                return;
            }

            currentLadderPath = other.transform.parent.Find("LadderPath");
            currentLadderStart = other.transform;
            currentLadderEnd = other.transform.parent.Find("LadderEnd");
            currentLadderSlipData =
                other.transform.parent.GetComponent<LadderSlipData>();

            if (currentLadderPath == null)
                return;

            Vector2 pathDirection = currentLadderPath.up.normalized;

            // 사다리가 오른쪽 위로 기울어졌는지 확인
            bool slopesRight = pathDirection.x >= 0;

            // 플레이어가 사다리 시작점의 어느 쪽에 있는지 확인
            float playerSide = transform.position.x - other.transform.position.x;

            // 오른쪽 위 사다리: 왼쪽에서 진입
            // 왼쪽 위 사다리: 오른쪽에서 진입
            bool canEnter =
                (slopesRight && playerSide < 0) ||
                (!slopesRight && playerSide > 0);

            if (!canEnter)
                return;

            isClimbing = true;

            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;

            transform.position = other.transform.position;
        }

        // LadderEnd에 진입하면 사다리 하차
        else if (other.CompareTag("LadderEnd"))
        {
            if (!isClimbing)
                return;

            isClimbing = false;
            currentLadderPath = null;
            currentLadderStart = null;
            currentLadderEnd = null;
            currentLadderSlipData = null;

            rb.gravityScale = originalGravityScale;
            rb.linearVelocity = Vector2.zero;
        }
    }
    public void ForceExitClimbing()
    {
        if (!isClimbing)
            return;

        isClimbing = false;
        currentLadderPath = null;
        currentLadderStart = null;
        currentLadderEnd = null;
        currentLadderSlipData = null;
        canMove = true;

        rb.gravityScale = originalGravityScale;
    }

    private bool CheckLadderSlip()
    {
        // 필요한 정보가 없으면 판정하지 않음
        if (slipState == null ||
            currentLadderSlipData == null ||
            !currentLadderSlipData.HasResult ||
            currentLadderStart == null ||
            currentLadderEnd == null)
        {
            return false;
        }

        SlipResult result = currentLadderSlipData.Result;

        // 안정적인 사다리라면 미끄러지지 않음
        if (result.isStable)
            return false;

        // 사다리의 시작점과 끝점
        Vector2 start = currentLadderStart.position;
        Vector2 end = currentLadderEnd.position;

        Vector2 ladderDirection = end - start;

        // 사다리 길이가 0이면 판정하지 않음
        if (ladderDirection.sqrMagnitude < 0.0001f)
            return false;

        // 플레이어의 현재 등반 진행도 계산 (0~1)
        float climbProgress = Vector2.Dot(
            (Vector2)transform.position - start,
            ladderDirection
        ) / ladderDirection.sqrMagnitude;

        climbProgress = Mathf.Clamp01(climbProgress);

        // 미끄러질 위치에 도달했는지 확인
        if (PlayerSlipState.ShouldSlip(climbProgress, result))
        {
            Vector2 pathDirection = ladderDirection.normalized;

            // 사다리 방향을 고려해 아래쪽으로 미끄러짐
            Vector2 slipDirection = new Vector2(1f, -0.3f);

            slipState.StartSlip(slipDirection);

            return true;
        }

        return false;
    }

}
