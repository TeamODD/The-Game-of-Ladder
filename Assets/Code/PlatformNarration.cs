using UnityEngine;

public class PlatformNarration : MonoBehaviour
{
    public bool[] Lower;
    public bool[] Middle;
    public bool[] Upper;

    private NarrationManager narrationManager;
    private Rigidbody2D playerRb;

    private void Awake()
    {
        narrationManager = FindAnyObjectByType<NarrationManager>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {

        // 플레이어인지 확인
        if (!collision.gameObject.CompareTag("Player"))
            return;

        playerRb = collision.rigidbody;

        if (playerRb == null)
            return;

        // 아래로 떨어지는 중이었는지 확인
        if (playerRb.linearVelocity.y > 0.05f)
            return;

        // 플랫폼 윗면에 착지했는지 확인
        bool isLanding = false;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
            {
                isLanding = true;
                break;
            }
        }

        if (!isLanding)
            return;

        for (int i = 0; i < Lower.Length; i++)
        {
            if (Lower[i])
            {
                narrationManager.ShowLowerNarration(i);
                Lower[i] = false;
                return;
            }
        }
        for (int i = 0; i < Middle.Length; i++)
        {
            if (Middle[i])
            {
                narrationManager.ShowMiddleNarration(i);
                Middle[i] = false;
                return;
            }
        }
        for (int i = 0; i < Upper.Length; i++)
        {
            if (Upper[i])
            {
                narrationManager.ShowUpperNarration(i);
                Upper[i] = false;
                return;
            }
        }
    }
}
