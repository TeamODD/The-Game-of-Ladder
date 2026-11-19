using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("ªÁ¥Ÿ∏Æ µÈ±‚ ¿ßƒ°")]
    public Transform leftCarryPoint;
    public Transform rightCarryPoint;
    private Transform carryPoint;

    private SpriteRenderer playerSpriteRenderer;
    private Vector3 ladderWorldScale;
    private float currentCarryAngle;
    private bool previousFacingLeft;

    private GameObject nearbyLadder;
    private GameObject carriedLadder;
    private Player Player;
    private Animator animator;

    public LadderPartsController ladderParts;

    [Header("Ladder Supply")]
    public GameObject ladderInSupply;
    private bool isNearLadderSupply = false;

    [Header("ªÛ»£¿€øÎ æ»≥ª")]
    public GameObject pickupPrompt;

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
        playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        // ªÁ¥Ÿ∏Æ∏¶ µÈ ºˆ ¿÷¥¬ ªÛ»≤¿Œ¡ˆ »Æ¿Œ
        bool canPickup =
            carriedLadder == null &&
            !Player.isClimbing &&
            (nearbyLadder != null || isNearLadderSupply);

        // æ»≥ªπÆ «•Ω√
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(canPickup);
        }

        if (carriedLadder != null)
            UpdateCarryPoint();
        

        if (Keyboard.current == null)
            return;


        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            if (carriedLadder == null)
            {
                if (isNearLadderSupply && ladderInSupply != null)
                {
                    PickUpLadder(ladderInSupply);
                    animator.SetBool("isPush", true);
                }
                else if (nearbyLadder != null)
                {
                    PickUpLadder(nearbyLadder);
                    animator.SetBool("isPush", true);
                }
            }
            else
            {
                animator.SetBool("isAd", true);
                animator.SetBool("isPush", false);

                ChangeMode();

                if (currentMode == LadderMode.Place)
                {
                    PlaceLadder();
                    animator.SetBool("isAd", false);
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


    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            nearbyLadder = other.gameObject;
        }
        if (other.CompareTag("LadderPath"))
        {
            nearbyLadder = other.transform.root.gameObject;
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
        if (other.CompareTag("LadderPath"))
        {
            if (other.transform.root.gameObject == nearbyLadder)
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

        ladderParts = carriedLadder.GetComponentInChildren<LadderPartsController>(true);

        if (ladderParts == null)
        {
            Debug.LogError("LadderPartsController∏¶ √£¡ˆ ∏¯«ﬂæÓ!");
            carriedLadder = null;
            return;
        }

        ladderParts.SetLength(ladderParts.CurrentLength);

        LadderSlipData slipData = carriedLadder.GetComponent<LadderSlipData>();

        if (slipData != null)
            slipData.ClearResult();

        // ªÁ¥Ÿ∏Æ¿« «ˆ¿Á ø˘µÂ ≈©±‚ ¿˙¿Â
        ladderWorldScale = carriedLadder.transform.lossyScale;

        // «√∑π¿ÃæÓ πÊ«‚ø° ∏¬¥¬ º’ º±≈√
        carryPoint = GetCarryPoint();
        previousFacingLeft = playerSpriteRenderer.flipX;

        // ªÁ¥Ÿ∏Æ ø¨∞·
        carriedLadder.transform.SetParent(carryPoint, false);
        carriedLadder.transform.localPosition = Vector3.zero;

        // ø˘µÂ ≈©±‚ ∫∏¡∏
        SetLadderWorldScale();

        // »∏¿¸ √ ±‚»≠
        currentCarryAngle = 0f;
        carryPoint.localRotation = Quaternion.identity;


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

        // Ï°∞Ï†à Î™®Îìú?êÏÑú???åÎ†à?¥Ïñ¥ ?¥Îèô ?úÌïú
        Player.canMove = (currentMode == LadderMode.Normal);
    }

    void AngleMode()
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
            input,
            Time.deltaTime
        );

        currentCarryAngle = carriedLadder.transform.localEulerAngles.z;

        if (currentCarryAngle > 180f)
            currentCarryAngle -= 360f;
    }

    void LengthMode()
    {
        if (ladderParts == null)
            return;

        float input = 0f;

        bool isFacingLeft = carryPoint.lossyScale.x < 0f;

        if (Keyboard.current.dKey.isPressed)
            input = isFacingLeft ? -1f : 1f;

        if (Keyboard.current.aKey.isPressed)
            input = isFacingLeft ? 1f : -1f;

        ladderParts.AdjustLength(input, Time.deltaTime);
    }

    void PlaceLadder()
    {
        if (carriedLadder == null)
            return;

        LadderSlipData slipData = carriedLadder.GetComponent<LadderSlipData>();

        if (slipData != null)
            slipData.EvaluatePlacement();

        // Î∂ÄÎ™?Í¥ÄÍ≥??¥Ï†ú
        carriedLadder.transform.SetParent(null, true);

        // Ï∂©Îèå ?úÏÑ±??
        Collider2D[] colliders = carriedLadder.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D col in colliders)
        {
            col.enabled = true;
            if (col.gameObject.CompareTag("LadderPath"))
            {
                col.isTrigger = true;
            }
        }

        // ?§Ïπò ??Î¨ºÎ¶¨?ÅÏúºÎ°??ÄÏßÅÏù¥ÏßÄ ?äÎèÑÎ°?Í≥†Ï†ï
        Rigidbody2D ladderRb = carriedLadder.GetComponent<Rigidbody2D>();

        if (ladderRb != null)
        {
            ladderRb.simulated = true;
            ladderRb.bodyType = RigidbodyType2D.Static;
        }

        carriedLadder = null;
        currentMode = LadderMode.Normal;
        Player.canMove = true;
    }
    void UpdateCarryPoint()
    {
        if (carriedLadder == null || playerSpriteRenderer == null)
            return;

        bool facingLeft = playerSpriteRenderer.flipX;

        Transform nextPoint = facingLeft
            ? leftCarryPoint
            : rightCarryPoint;

        if (carryPoint == nextPoint)
            return;

        // πÊ«‚ ¿¸»Ø Ω√ ∞¢µµ π›¿¸
        float currentAngle = carriedLadder.transform.localEulerAngles.z;

        if (currentAngle > 180f)
            currentAngle -= 360f;

        currentCarryAngle = -currentAngle;

        // ªı∑ŒøÓ º’¿∏∑Œ ¿Ãµø
        carryPoint = nextPoint;

        carriedLadder.transform.SetParent(carryPoint, false);
        carriedLadder.transform.localPosition = Vector3.zero;

        SetLadderWorldScale();

        // ªÁ¥Ÿ∏Æ ¿⁄√ºø° »∏¿¸ ¿˚øÎ
        carriedLadder.transform.localRotation = Quaternion.Euler(
            0f, 0f, currentCarryAngle
        );
    }

    Transform GetCarryPoint()
    {
        bool facingLeft = playerSpriteRenderer.flipX;

        return facingLeft ? leftCarryPoint : rightCarryPoint;
    }
    void SetLadderWorldScale()
    {
        Vector3 parentScale = carriedLadder.transform.parent.lossyScale;

        carriedLadder.transform.localScale = new Vector3(
            ladderWorldScale.x / parentScale.x,
            ladderWorldScale.y / parentScale.y,
            ladderWorldScale.z / parentScale.z
        );
    }
}