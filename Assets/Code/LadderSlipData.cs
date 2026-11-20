
using UnityEngine;

public class LadderSlipData : MonoBehaviour
{
    [Header("판정 수치")]
    public SlipSettings settings = SlipSettings.Default;
    public float zoneSearchRadius = 1.5f;

    [Header("디버그")]
    public bool logResult = true;

    [Header("LadderEnd")]
    public Transform ladderEnd;

    private const float ladderEndExtraHeight = 0.6f;

    public SlipResult Result { get; private set; }
    public bool HasResult { get; private set; }
    public LadderAnchorZone TargetZone { get; private set; }

    private Transform ladderStart;
    private Transform ladderCheck;

    private void Awake()
    {
        ladderStart = FindChildWithTag("LadderStart");
        ladderCheck = FindChildWithTag("LadderCheck");

        if (ladderStart == null || ladderCheck == null || ladderEnd == null)
        {
            Debug.LogWarning(
                $"[LadderSlipData] {name}: LadderStart/LadderCheck/LadderEnd를 찾지 못함"
            );
        }
    }

    // 사다리 설치 판정
    public SlipResult EvaluatePlacement()
    {
        if (ladderStart == null || ladderCheck == null)
        {
            Result = New_LadderSlipCalculator.Unstable(0f);
            HasResult = true;
            return Result;
        }

        Vector2 start = ladderStart.position;
        Vector2 direction = (Vector2)ladderCheck.position - start;

        float length = direction.magnitude;
        float angleDeg = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Vector2 end = New_LadderSlipCalculator.GetEndPoint(
            start,
            angleDeg,
            length
        );

        TargetZone = LadderAnchorZone.FindNearest(end, zoneSearchRadius);

        if (TargetZone == null)
        {
            Result = New_LadderSlipCalculator.Unstable(settings.minSlipPoint);
        }
        else
        {
            Result = New_LadderSlipCalculator.Evaluate(
                start,
                angleDeg,
                length,
                TargetZone.GetWorldRect(),
                settings
            );
        }

        HasResult = true;

        PositionLadderEnd();

        if (ladderEnd != null)
            ladderEnd.gameObject.SetActive(Result.isStable);

        if (logResult)
        {
            Debug.Log(
                $"[LadderSlipData] 안정: {Result.isStable}, 미끄러짐 지점: {Result.slipPoint:F2}"
            );
        }

        return Result;
    }

    // 사다리 회수 시 판정 초기화
    public void ClearResult()
    {
        HasResult = false;
        TargetZone = null;
    }

    // 자식 오브젝트에서 태그로 Transform 검색
    private Transform FindChildWithTag(string tag)
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.CompareTag(tag))
                return child;
        }

        return null;
    }

    // 판정 결과 시각화
    private void OnDrawGizmos()
    {
        if (!HasResult || ladderStart == null || ladderCheck == null)
            return;

        Vector3 start = ladderStart.position;
        Vector3 end = ladderCheck.position;

        Gizmos.color = Result.isStable ? Color.green : Color.red;
        Gizmos.DrawLine(start, end);

        if (!Result.isStable)
        {
            Gizmos.DrawSphere(
                Vector3.Lerp(start, end, Result.slipPoint),
                0.08f
            );
        }
    }

    // 플레이어 머리보다 약간 높은 위치에 LadderEnd 배치
    private void PositionLadderEnd()
    {
        if (ladderEnd == null || ladderCheck == null)
            return;

        Player player = FindAnyObjectByType<Player>();

        if (player == null)
            return;

        CapsuleCollider2D playerCollider =
            player.GetComponent<CapsuleCollider2D>();

        if (playerCollider == null)
            return;

        float playerHeight = playerCollider.bounds.size.y;

        ladderEnd.position = ladderCheck.position +
            Vector3.up * (playerHeight + ladderEndExtraHeight);

        ladderEnd.rotation = Quaternion.identity;
    }
}
