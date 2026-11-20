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
