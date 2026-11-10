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
    private Animator animator;



    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
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
        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );
    }
}
