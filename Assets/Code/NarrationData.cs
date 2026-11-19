
using UnityEngine;

[CreateAssetMenu(
    fileName = "NarrationData",
    menuName = "Game/Narration Data"
)]
public class NarrationData : ScriptableObject
{
    [Header("나레이션 정보")]
    public string narrationID;

    [TextArea(3, 6)]
    public string narrationText;
}
