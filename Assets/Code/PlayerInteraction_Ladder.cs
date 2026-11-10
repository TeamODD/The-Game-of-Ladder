using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public Transform carryPoint;

    private GameObject nearbyLadder;
    private GameObject carriedLadder;
    private Player Player; 

    private bool isAngle = false;

    public float rotateSpeed = 100f;

    private void Awake()
    {
        Player = GetComponent<Player>();
    }
    void Update()
    {
        // W키를 눌렀을 때
        if (Keyboard.current != null &&
            Keyboard.current.wKey.wasPressedThisFrame)
        {
            if (carriedLadder == null)
            {
                PickUpLadder();
            }
            else if (!isAngle)
            {
                ToggleAngleMode();
            }
        }

        if (isAngle)
        {
            AngleMode();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            nearbyLadder = other.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == nearbyLadder)
        {
            nearbyLadder = null;
        }
    }

    void PickUpLadder()
    {
        if (nearbyLadder == null || carriedLadder != null)
            return;

        carriedLadder = nearbyLadder;
        nearbyLadder = null;

        SpriteRenderer sr = carriedLadder.GetComponent<SpriteRenderer>();

        carriedLadder.transform.SetParent(carryPoint);
        carriedLadder.transform.localPosition = Vector3.zero;

        if (sr != null)
        {
            float halfHeight = sr.bounds.extents.y;

            carriedLadder.transform.localPosition =
                new Vector3(0, halfHeight, 0);
        }

        Rigidbody2D ladderRb =
            carriedLadder.GetComponent<Rigidbody2D>();

        if (ladderRb != null)
            ladderRb.simulated = false;

        Collider2D ladderCollider =
            carriedLadder.GetComponent<Collider2D>();

        if (ladderCollider != null)
            ladderCollider.enabled = false;
    }

    void ToggleAngleMode()
    {
        isAngle = !isAngle;
        Player.canMove = !isAngle;
    }

    void AngleMode()
    {
        // 각도 조절 모드
        if (isAngle && carriedLadder != null)
        {
            float rotationInput = 0f;

            if (Keyboard.current.aKey.isPressed)
                rotationInput = 1f;

            if (Keyboard.current.dKey.isPressed)
                rotationInput = -1f;

            carryPoint.Rotate(
                0f,
                0f,
                rotationInput * rotateSpeed * Time.deltaTime
            );
        }
    }
}