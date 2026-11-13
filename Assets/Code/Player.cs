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


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
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

            rb.gravityScale = originalGravityScale;
            rb.linearVelocity = Vector2.zero;
        }
    }

}
