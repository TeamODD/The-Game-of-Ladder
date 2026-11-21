
using UnityEngine;

public class PlatformNarration : MonoBehaviour
{
    public bool[] Lower;
    public bool[] Middle;
    public bool[] Upper;

    private NarrationManager narrationManager;

    private void Awake()
    {
        narrationManager = FindAnyObjectByType<NarrationManager>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        Rigidbody2D playerRb = collision.rigidbody;

        if (playerRb == null || playerRb.linearVelocity.y > 0.05f)
            return;

        // 플랫폼 윗면 착지 확인
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

        // 하층 나레이션
        for (int i = 0; i < Lower.Length; i++)
        {
            if (Lower[i])
            {
                narrationManager.ShowLowerNarration(i);
                DisableSiblingNarrations(0, i);
                return;
            }
        }

        // 중층 나레이션
        for (int i = 0; i < Middle.Length; i++)
        {
            if (Middle[i])
            {
                narrationManager.ShowMiddleNarration(i);
                DisableSiblingNarrations(1, i);
                return;
            }
        }

        // 상층 나레이션
        for (int i = 0; i < Upper.Length; i++)
        {
            if (Upper[i])
            {
                narrationManager.ShowUpperNarration(i);
                DisableSiblingNarrations(2, i);
                return;
            }
        }
    }

    // 같은 부모 아래에서 해당 나레이션 번호만 비활성화
    private void DisableSiblingNarrations(int layer, int index)
    {
        if (transform.parent == null)
        {
            DisableNarration(layer, index);
            return;
        }

        PlatformNarration[] siblingPlatforms =
            transform.parent.GetComponentsInChildren<PlatformNarration>(true);

        foreach (PlatformNarration platform in siblingPlatforms)
        {
            if (platform.transform.parent != transform.parent)
                continue;

            platform.DisableNarration(layer, index);
        }
    }

    // 지정된 층과 번호만 비활성화
    private void DisableNarration(int layer, int index)
    {
        switch (layer)
        {
            case 0:
                if (index < Lower.Length)
                    Lower[index] = false;
                break;

            case 1:
                if (index < Middle.Length)
                    Middle[index] = false;
                break;

            case 2:
                if (index < Upper.Length)
                    Upper[index] = false;
                break;
        }
    }
}
