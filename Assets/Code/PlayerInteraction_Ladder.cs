using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public Transform carryPoint;

    private GameObject nearbyLadder;
    private GameObject carriedLadder;
    private Player Player;

    [Header("ªÁ¥Ÿ∏Æ ¡∂¿˝")]
    public float lengthChangeSpeed = 1f;
    public float minLengthScale = 0.5f;
    public float maxLengthScale = 2.5f;
    [Header("Ladder Rotation")]
    public float rotateSpeed = 100f;
    public float minRotation = -90f;
    public float maxRotation = 90f;
    

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
        // W?§Î? ?åÎ?????
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

        LadderSlipData slipData = carriedLadder.GetComponent<LadderSlipData>();
        if (slipData != null)
            slipData.ClearResult();

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

        // Ï°∞Ï†à Î™®Îìú?êÏÑú???åÎ†à?¥Ïñ¥ ?¥Îèô ?úÌïú
        Player.canMove = (currentMode == LadderMode.Normal);
    }

    void AngleMode()
    {
        // Í∞ÅÎèÑ Ï°∞Ï†à Î™®Îìú
        float rotationInput = 0f;

        if (Keyboard.current.aKey.isPressed)
            rotationInput = 1f;

        if (Keyboard.current.dKey.isPressed)
            rotationInput = -1f;

        // «ˆ¿Á ∞¢µµ ∞°¡Æø¿±‚
        float currentAngle = carryPoint.localEulerAngles.z;

        // 0~360µµ∏¶ -180~180µµ∑Œ ∫Ø»Ø
        if (currentAngle > 180f)
            currentAngle -= 360f;

        // »∏¿¸ ∞¢µµ ∞ËªÍ
        float nextAngle = currentAngle +
            rotationInput * rotateSpeed * Time.deltaTime;

        // -90µµ ~ 90µµ∑Œ ¡¶«—
        nextAngle = Mathf.Clamp(nextAngle, -90f, 90f);

        // »∏¿¸ ¿˚øÎ
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
        LadderSlipData slipData = carriedLadder.GetComponent<LadderSlipData>();
        if (slipData != null)
            slipData.EvaluatePlacement();

        if (carriedLadder == null)
            return;

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

}