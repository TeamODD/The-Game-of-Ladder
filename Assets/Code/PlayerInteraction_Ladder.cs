using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public Transform carryPoint;

    private GameObject nearbyLadder;
    private GameObject carriedLadder;
    private Player Player;

    [Header("사다리 조절")]
    public float lengthChangeSpeed = 1f;
    public float minLengthScale = 0.5f;
    public float maxLengthScale = 2.5f;
    [Header("Ladder Rotation")]
    public float rotateSpeed = 100f;
    public float minRotation = -90f;
    public float maxRotation = 90f;
    private float currentRotation = 0f;
    

    private Vector3 ladderBaseScale;
    private float currentLengthScale = 1f;

    [Header("Ladder Supply")]
    public GameObject ladderInSupply;
    private bool isNearLadderSupply = false;

    public enum LadderMode
    {
        Normal,
        Angle,
        Length,
        Place
    }

    private LadderMode currentMode = LadderMode.Normal;



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
                if (isNearLadderSupply && ladderInSupply != null)
                {
                    PickUpLadder(ladderInSupply);
                }
                else if (nearbyLadder != null)
                {
                    PickUpLadder(nearbyLadder);
                }
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
        if (other.CompareTag("LadderSupply"))
        {
            isNearLadderSupply = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == nearbyLadder)
        {
            nearbyLadder = null;
        }
        if (other.CompareTag("LadderSupply"))
        {
            isNearLadderSupply = false;
        }
    }

    void PickUpLadder(GameObject targetLadder)
    {
        if (targetLadder == null || carriedLadder != null || Player.isClimbing)
            return;

        carriedLadder = targetLadder;
        targetLadder = null;

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
        if (Player.isClimbing)
            return;
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

        // 현재 각도 가져오기
        float currentAngle = carryPoint.localEulerAngles.z;

        // 0~360도를 -180~180도로 변환
        if (currentAngle > 180f)
            currentAngle -= 360f;

        // 회전 각도 계산
        float nextAngle = currentAngle +
            rotationInput * rotateSpeed * Time.deltaTime;

        // -90도 ~ 90도로 제한
        nextAngle = Mathf.Clamp(nextAngle, -90f, 90f);

        // 회전 적용
        carryPoint.localRotation = Quaternion.Euler(0f, 0f, nextAngle);

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