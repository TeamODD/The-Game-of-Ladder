// New_LadderSlipCalculator.cs
// 충돌 방지 때문에 새로 만듦 (오브젝트에 붙이는 용도 아님)
using UnityEngine;

// 계산 결과를 담는 구조체
public struct SlipResult
{
    public bool isStable;  // 사다리가 안정적으로 설치되어있는지
    public float slipPoint;  // 미끄러지기 시작하는 지점 (0~1, 사다리 길이 비율)
}

// 판정 수치 (수정 필요)
[System.Serializable]
public struct SlipSettings
{
    [Tooltip("판정 사각형 바깥으로 이만큼까지는 안정으로 인정 (월드 단위)")]
    public float tolerance;

    [Tooltip("이 거리 이상 벗어나면 가장 일찍 미끄러짐 (월드 단위)")]
    public float maxSlipDistance;

    [Tooltip("아주 조금 벗어났을 때 미끄러지는 지점 (0~1)")]
    [Range(0f, 1f)] public float maxSlipPoint;

    [Tooltip("크게 벗어났을 때 미끄러지는 지점 (0~1)")]
    [Range(0f, 1f)] public float minSlipPoint;

    // 임의 기본값 (수정 필요)
    public static SlipSettings Default => new SlipSettings
    {
        tolerance = 0.05f,
        maxSlipDistance = 0.5f,
        maxSlipPoint = 0.8f,
        minSlipPoint = 0.2f
    };
}

public static class New_LadderSlipCalculator
{
    // 끝점x = 시작점x + L * cosθ
    // 끝점y = 시작점y + L * sinθ
    public static Vector2 GetEndPoint(Vector2 start, float angleDeg, float length)
    {
        float rad = angleDeg * Mathf.Deg2Rad;  // 각을 호도법으로 표현
        return start + length * new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    /*
    Evaluate의 매개변수 설명
    - start: 사다리 아래쪽 끝의 월드 좌표
    - angleDeg: 사다리 각도. +X축(오른쪽) 기준 0° (반시계 방향이 '+'. 수직은 90°)
    - length: 사다리의 월드 단위 실제 길이(스케일 배율 아님)
    - zone: 플랫폼 모서리 판정 사각형 (월드 좌표)
    - settings: 판정 수치
    */
    public static SlipResult Evaluate(Vector2 start, float angleDeg, float length,
                                      Rect zone, SlipSettings settings)
    {
        Vector2 end = GetEndPoint(start, angleDeg, length);
        float outside = DistanceOutside(zone, end);

        // 사각형 안 또는 오차 범위 안 → 안정
        if (outside <= settings.tolerance)
            return Stable();

        // 벗어난 거리에 비례해 미끄러지는 지점을 앞당김
        float t = Mathf.InverseLerp(settings.tolerance, settings.maxSlipDistance, outside);
        return Unstable(Mathf.Lerp(settings.maxSlipPoint, settings.minSlipPoint, t));
    }

    // 점이 사각형 바깥으로 벗어난 거리 (안에 있으면 0)
    public static float DistanceOutside(Rect zone, Vector2 point)
    {
        float dx = Mathf.Max(zone.xMin - point.x, 0f, point.x - zone.xMax);
        float dy = Mathf.Max(zone.yMin - point.y, 0f, point.y - zone.yMax);
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    public static SlipResult Stable()
    {
        return new SlipResult { isStable = true, slipPoint = 1f };
    }

    public static SlipResult Unstable(float slipPoint)
    {
        return new SlipResult { isStable = false, slipPoint = Mathf.Clamp01(slipPoint) };
    }
}