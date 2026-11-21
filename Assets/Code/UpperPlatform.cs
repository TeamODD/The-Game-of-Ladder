using UnityEngine;

public class UpperPlatform : MonoBehaviour
{
    LadderPartsController ladderParts;
    
    public bool dontSkip = false;

    private void Awake()
    {
        ladderParts = FindAnyObjectByType<LadderPartsController>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (dontSkip)
        {
            ladderParts.maxLength = 2.75f;
        }
        else ladderParts.maxLength = 3.5f;
    }
}
