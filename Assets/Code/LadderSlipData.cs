// LadderSlipData.cs
// 사다리 설치 판정 실행과 결과 보관 (사다리 오브젝트에 추가)
// 사다리 설치 시 EvaluatePlacement()를 호출. 등반 코드는 result를 읽어 사용함
using System.Collections.Generic;
using UnityEngine;

public class LadderSlipData : MonoBehaviour
{
[Header("판정 수치")]
    public SlipSettings settings = SlipSettings.Default;
    public float zoneSearchRadius = 1.5f;  // 끝점에서 이 거리 안의 판정 사각형만 찾음

    [Header("디버그")]
    public bool logResult = true;

    public SlipResult Result { get; private set; }
    public bool HasResult { get; private set; }
    public LadderAnchorZone TargetZone { get; private set; }

    private Transform ladderStart;
    private Transform ladderEnd;

    private void Awake()
    {
        ladderStart = FindChildWithTag("LadderStart");
        ladderEnd = FindChildWithTag("LadderEnd");

        if (ladderStart == null || ladderEnd == null)
            Debug.LogWarning($"[LadderSlipData] {name}: LadderStart/LadderEnd 자식을 찾지 못함");
    }

    // 설치 직후 호출 (PlaceLadder에서)
    public SlipResult EvaluatePlacement()
    {
        if (ladderStart == null || ladderEnd == null)
        {
            Result = New_LadderSlipCalculator.Unstable(0f);
            HasResult = true;
            return Result;
        }

        // 사다리 양 끝 → 계산기 입력값 (각도, 월드 길이)
        Vector2 start = ladderStart.position;
        Vector2 dir = (Vector2)ladderEnd.position - start;
        float length = dir.magnitude;
        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Vector2 end = New_LadderSlipCalculator.GetEndPoint(start, angleDeg, length);
        TargetZone = LadderAnchorZone.FindNearest(end, zoneSearchRadius);

        if (TargetZone == null)
            Result = New_LadderSlipCalculator.Unstable(settings.minSlipPoint);  // 근처에 모서리 없음
        else
            Result = New_LadderSlipCalculator.Evaluate(start, angleDeg, length,
                                                   TargetZone.GetWorldRect(), settings);

        HasResult = true;

        if (logResult)
            Debug.Log($"[LadderSlipData] 안정: {Result.isStable}, 미끄러짐 지점: {Result.slipPoint:F2}");

        return Result;
    }

    // 회수 시 호출 (이전 판정 초기화)
    public void ClearResult()
    {
        HasResult = false;
        TargetZone = null;
    }

    private Transform FindChildWithTag(string tag)
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.CompareTag(tag))
                return child;
        }
        return null;
    }

    // 판정 결과 표시: 초록 = 안정, 빨강 = 불안정 (빨간 점 = 미끄러지는 지점)
    private void OnDrawGizmos()
    {
        if (!HasResult || ladderStart == null || ladderEnd == null)
            return;

        Vector3 a = ladderStart.position;
        Vector3 b = ladderEnd.position;

        Gizmos.color = Result.isStable ? Color.green : Color.red;
        Gizmos.DrawLine(a, b);

        if (!Result.isStable)
            Gizmos.DrawSphere(Vector3.Lerp(a, b, Result.slipPoint), 0.08f);
    }
}
