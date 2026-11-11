using Unity.VisualScripting.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;

public class Player : MonoBehaviour
{
    public float moveSpeed;
    public bool canMove = true;


    private Rigidbody2D rb;
    private int moveInput;
    private SpriteRenderer sprite;
    private float originalGravityScale;
    private bool isClimbing = false;

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
        if (!other.CompareTag("LadderStart"))
            return;

        if (!isClimbing)
        {
            // 사다리 탑승
            isClimbing = true;

            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;

            // LadderStart에 연결된 LadderPath 찾기
            currentLadderPath = other.transform.parent
                .Find("LadderPath");

            if (currentLadderPath != null)
            {
                transform.position = other.transform.position;
            }
        }
        else
        {
            // 사다리에서 내리기
            isClimbing = false;

            currentLadderPath = null;

            rb.gravityScale = originalGravityScale;
            rb.linearVelocity = Vector2.zero;
        }
    }


}
