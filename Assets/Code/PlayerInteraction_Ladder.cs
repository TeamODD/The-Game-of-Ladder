
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("사다리 들기 위치")]
    public Transform leftCarryPoint;
    public Transform rightCarryPoint;

    [Header("사다리 공급")]
    public GameObject ladderInSupply;

    [Header("상호작용 안내")]
    public GameObject pickupPrompt;

    private Player player;
    private Animator animator;
    private SpriteRenderer playerSpriteRenderer;

    private Transform carryPoint;

    private GameObject nearbyLadder;
    private GameObject carriedLadder;

    private LadderPartsController ladderParts;

    private Vector3 ladderWorldScale;
    private float currentCarryAngle;

    private bool isNearLadderSupply;

    public enum LadderMode
    {
        Normal,
        Angle,
        Length,
        Place
    }

    private LadderMode currentMode = LadderMode.Normal;


    // =========================
    // 초기화
    // =========================

    private void Awake()
    {
        player = GetComponent<Player>();
        animator = GetComponent<Animator>();
        playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }


    // =========================
    // 업데이트
    // =========================

    private void Update()
    {
        UpdatePickupPrompt();

        if (carriedLadder != null)
            UpdateCarryPoint();

        if (Keyboard.current == null)
            return;

        HandleInteractionInput();

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


    private void UpdatePickupPrompt()
    {
        if (pickupPrompt == null)
            return;

        bool canPickup =
            carriedLadder == null &&
            !player.isClimbing &&
            (nearbyLadder != null || isNearLadderSupply);

        pickupPrompt.SetActive(canPickup);
    }


    private void HandleInteractionInput()
    {
        if (!Keyboard.current.wKey.wasPressedThisFrame)
            return;

        // 사다리를 들고 있지 않은 경우
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

            return;
        }

        // 사다리를 들고 있는 경우
        if (animator != null)
        {
            animator.SetBool("isAd", true);
            animator.SetBool("isPush", false);
        }

        ChangeMode();

        if (currentMode == LadderMode.Place)
            PlaceLadder();
    }


    // =========================
    // 주변 사다리 감지
    // =========================

    private void OnTriggerStay2D(Collider2D other)
    {
        GameObject ladderObject = GetLadderObject(other);

        if (ladderObject != null)
            nearbyLadder = ladderObject;

        if (other.CompareTag("LadderSupply"))
            isNearLadderSupply = true;
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        GameObject ladderObject = GetLadderObject(other);

        if (ladderObject != null && ladderObject == nearbyLadder)
            nearbyLadder = null;

        if (other.CompareTag("LadderSupply"))
            isNearLadderSupply = false;
    }


    private GameObject GetLadderObject(Collider2D other)
    {
        // LadderPath 등의 자식 오브젝트에서
        // LadderSlipData를 가진 Ladder를 찾음
        LadderSlipData slipData =
            other.GetComponentInParent<LadderSlipData>();

        if (slipData != null)
            return slipData.gameObject;

        // 일반 Ladder 오브젝트
        if (other.CompareTag("Ladder"))
            return other.gameObject;

        return null;
    }


    // =========================
    // 사다리 줍기
    // =========================

    private void PickUpLadder(GameObject targetLadder)
    {
        if (targetLadder == null ||
            carriedLadder != null ||
            player.isClimbing)
        {
            return;
        }

        if (leftCarryPoint == null ||
            rightCarryPoint == null ||
            playerSpriteRenderer == null)
        {
            Debug.LogError("CarryPoint 또는 SpriteRenderer가 연결되지 않았습니다.");
            return;
        }

        // 사다리 참조 저장
        carriedLadder = targetLadder;

        // 주변 감지 정보 초기화
        nearbyLadder = null;
        isNearLadderSupply = false;

        // 사다리 구성요소 찾기
        ladderParts =
            carriedLadder.GetComponentInChildren<LadderPartsController>(true);

        if (ladderParts == null)
        {
            Debug.LogError("LadderPartsController를 찾지 못했습니다.");

            carriedLadder = null;
            return;
        }

        ladderParts.SetLength(ladderParts.CurrentLength);

        // 사다리 설치 판정 초기화
        LadderSlipData slipData =
            carriedLadder.GetComponent<LadderSlipData>();

        if (slipData != null)
        {
            slipData.ClearResult();

            if (slipData.ladderEnd != null)
                slipData.ladderEnd.gameObject.SetActive(false);
        }

        // 현재 월드 크기 저장
        ladderWorldScale = carriedLadder.transform.lossyScale;

        // 플레이어 방향에 맞는 손 선택
        carryPoint = GetCarryPoint();

        // 사다리 연결
        carriedLadder.transform.SetParent(carryPoint, false);
        carriedLadder.transform.localPosition = Vector3.zero;

        SetLadderWorldScale();

        // 들었을 때 초기 각도
        currentCarryAngle = 0f;
        carriedLadder.transform.localRotation = Quaternion.identity;

        // 물리 비활성화
        Rigidbody2D ladderRb =
            carriedLadder.GetComponent<Rigidbody2D>();

        if (ladderRb != null)
            ladderRb.simulated = false;

        // 충돌체 비활성화
        Collider2D[] colliders =
            carriedLadder.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D col in colliders)
        {
            col.enabled = false;

            if (col.CompareTag("LadderPath"))
                col.isTrigger = true;
        }

        if (animator != null)
            animator.SetBool("isPush", true);
    }


    // =========================
    // 조작 모드 변경
    // =========================

    private void ChangeMode()
    {
        if (player.isClimbing)
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

        // 조절 중에는 플레이어 이동 제한
        player.canMove = currentMode == LadderMode.Normal;
    }


    // =========================
    // 각도 조절
    // =========================

    private void AngleMode()
    {
        if (ladderParts == null || carriedLadder == null)
            return;

        float input = 0f;

        if (Keyboard.current.aKey.isPressed)
            input = 1f;

        if (Keyboard.current.dKey.isPressed)
            input = -1f;

        ladderParts.AdjustRotation(
            carriedLadder.transform,
            input * player.SpeedMultiplier,
            Time.deltaTime
        );

        // 현재 각도 저장
        currentCarryAngle =
            carriedLadder.transform.localEulerAngles.z;

        if (currentCarryAngle > 180f)
            currentCarryAngle -= 360f;
    }


    // =========================
    // 길이 조절
    // =========================

    private void LengthMode()
    {
        if (ladderParts == null ||
            playerSpriteRenderer == null)
        {
            return;
        }

        float input = 0f;

        bool isFacingLeft = playerSpriteRenderer.flipX;

        if (Keyboard.current.dKey.isPressed)
            input = isFacingLeft ? -1f : 1f;

        if (Keyboard.current.aKey.isPressed)
            input = isFacingLeft ? 1f : -1f;

        ladderParts.AdjustLength(
    input * player.SpeedMultiplier,
    Time.deltaTime
);
    }


    // =========================
    // 사다리 설치
    // =========================

    private void PlaceLadder()
    {
        if (carriedLadder == null)
            return;

        // 부모 관계 해제
        // 월드 위치와 회전 유지
        carriedLadder.transform.SetParent(null, true);

        // 설치 판정
        LadderSlipData slipData =
            carriedLadder.GetComponent<LadderSlipData>();

        if (slipData != null)
            slipData.EvaluatePlacement();

        // 충돌체 활성화
        Collider2D[] colliders =
            carriedLadder.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D col in colliders)
        {
            col.enabled = true;

            if (col.CompareTag("LadderPath"))
                col.isTrigger = true;
        }

        // 물리 고정
        Rigidbody2D ladderRb =
            carriedLadder.GetComponent<Rigidbody2D>();

        if (ladderRb != null)
        {
            ladderRb.simulated = true;
            ladderRb.bodyType = RigidbodyType2D.Static;
        }

        // 상태 초기화
        carriedLadder = null;
        ladderParts = null;
        carryPoint = null;

        currentCarryAngle = 0f;
        currentMode = LadderMode.Normal;

        player.canMove = true;

        if (animator != null)
        {
            animator.SetBool("isAd", false);
            animator.SetBool("isPush", false);
        }

        // 주변 감지 정보 초기화
        nearbyLadder = null;
    }


    // =========================
    // 들고 있는 사다리 위치 갱신
    // =========================

    private void UpdateCarryPoint()
    {
        if (carriedLadder == null ||
            playerSpriteRenderer == null)
        {
            return;
        }

        bool facingLeft = playerSpriteRenderer.flipX;

        Transform nextPoint = GetCarryPoint();

        if (nextPoint == null || carryPoint == nextPoint)
            return;

        // 방향 전환 시 각도 반전
        currentCarryAngle = -currentCarryAngle;

        carryPoint = nextPoint;

        // 새로운 손에 연결
        carriedLadder.transform.SetParent(carryPoint, false);
        carriedLadder.transform.localPosition = Vector3.zero;

        // 월드 크기 유지
        SetLadderWorldScale();

        // 기존 각도를 반전하여 적용
        carriedLadder.transform.localRotation =
            Quaternion.Euler(0f, 0f, currentCarryAngle);
    }


    private Transform GetCarryPoint()
    {
        if (playerSpriteRenderer == null)
            return null;

        return playerSpriteRenderer.flipX
            ? leftCarryPoint
            : rightCarryPoint;
    }


    // =========================
    // 월드 크기 유지
    // =========================

    private void SetLadderWorldScale()
    {
        if (carriedLadder == null ||
            carriedLadder.transform.parent == null)
        {
            return;
        }

        Vector3 parentScale =
            carriedLadder.transform.parent.lossyScale;

        // 0으로 나누는 상황 방지
        if (Mathf.Approximately(parentScale.x, 0f) ||
            Mathf.Approximately(parentScale.y, 0f) ||
            Mathf.Approximately(parentScale.z, 0f))
        {
            Debug.LogWarning("CarryPoint의 Scale을 확인해주세요.");
            return;
        }

        carriedLadder.transform.localScale = new Vector3(
            ladderWorldScale.x / parentScale.x,
            ladderWorldScale.y / parentScale.y,
            ladderWorldScale.z / parentScale.z
        );
    }
}
