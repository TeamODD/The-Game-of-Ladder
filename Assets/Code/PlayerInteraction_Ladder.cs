using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public Transform carryPoint;

    private GameObject nearbyLadder;
    private GameObject carriedLadder;
    private Player Player;

    [Header("Ladder Length")]
    public float lengthChangeSpeed = 1f;
    public float minLengthScale = 0.5f;
    public float maxLengthScale = 2.5f;

    private Vector3 ladderBaseScale;
    private float currentLengthScale = 1f;


    public enum LadderMode
    {
        Normal,
        Angle,
        Length,
        Place
    }

    private LadderMode currentMode = LadderMode.Normal;

    public LayerMask groundLayer;
    public float placeRayDistance = 20f;


    public float rotateSpeed = 100f;

    private void Awake()
    {
        Player = GetComponent<Player>();
    }
    void Update()
    {
        // W키를 눌렀을 때
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            if (carriedLadder == null)
            {
                PickUpLadder();
            }
            else
            {
                ChangeMode();

                if (currentMode == LadderMode.Place)
                {
                    PlaceLadder();
                }
            }
        }

        switch (currentMode)
        { 
            case LadderMode.Angle:
                AngleMode();
                break;

            case LadderMode.Length:
                LengthMode();
                break;

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

        ladderBaseScale = carriedLadder.transform.localScale;
        currentLengthScale = 1f;

        Rigidbody2D ladderRb =
            carriedLadder.GetComponent<Rigidbody2D>();

        if (ladderRb != null)
            ladderRb.simulated = false;

        Collider2D[] colliders =
            carriedLadder.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
            if (col.gameObject.CompareTag("LadderPath"))
            {
                col.isTrigger = true;
            }
        }
    }

    void ChangeMode()
    {
        switch (currentMode)
        {
            case LadderMode.Normal:
                currentMode = LadderMode.Angle;
                break;

            case LadderMode.Angle:
                currentMode = LadderMode.Length;
                break;

            case LadderMode.Length:
                currentMode = LadderMode.Place;
                break;
        }

        // 조절 모드에서는 플레이어 이동 제한
        Player.canMove = (currentMode == LadderMode.Normal);
    }

    void AngleMode()
    {
        // 각도 조절 모드
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

    void LengthMode()
    {
        float input = 0f;

        if (Keyboard.current.dKey.isPressed)
            input = 1f;

        if (Keyboard.current.aKey.isPressed)
            input = -1f;

        currentLengthScale +=
            input * lengthChangeSpeed * Time.deltaTime;

        currentLengthScale = Mathf.Clamp(
            currentLengthScale,
            minLengthScale,
            maxLengthScale
        );

        Vector3 newScale = ladderBaseScale;

        newScale.y = ladderBaseScale.y * currentLengthScale;

        carriedLadder.transform.localScale = newScale;
    }

    void PlaceLadder()
    {
        if (carriedLadder == null)
            return;


        // 부모 관계 해제
        carriedLadder.transform.SetParent(null, true);

        // 충돌 활성화
        Collider2D[] colliders =
            carriedLadder.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D col in colliders)
        {
            col.enabled = true;
            if (col.gameObject.CompareTag("LadderPath"))
            {
                col.isTrigger = true;
            }
        }

        // 설치 후 물리적으로 움직이지 않도록 고정
        Rigidbody2D ladderRb =
            carriedLadder.GetComponent<Rigidbody2D>();

        if (ladderRb != null)
        {
            ladderRb.simulated = true;
            ladderRb.bodyType = RigidbodyType2D.Static;
        }

        carriedLadder = null;
        currentMode = LadderMode.Normal;
        Player.canMove = true;
    }

}