
using UnityEngine;

// 판정 결과
public struct SlipResult
{
    public bool isStable;
    public float slipPoint;
}

// 미끄러짐 판정 설정
[System.Serializable]
public struct SlipSettings
{
    [Tooltip("판정 사각형 바깥으로 이만큼까지는 안정으로 인정 (월드 단위)")]
    public float tolerance;

    [Tooltip("이 거리 이상 벗어나면 가장 일찍 미끄러짐 (월드 단위)")]
    public float maxSlipDistance;

    [Tooltip("아주 조금 벗어났을 때 미끄러지는 지점 (0~1)")]
    [Range(0f, 1f)]
    public float maxSlipPoint;

    [Tooltip("크게 벗어났을 때 미끄러지는 지점 (0~1)")]
    [Range(0f, 1f)]
    public float minSlipPoint;

    // 기본 판정 설정
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
    // 사다리 시작점과 각도, 길이를 이용해 끝점 계산
    public static Vector2 GetEndPoint(Vector2 start, float angleDeg, float length)
    {
        float rad = angleDeg * Mathf.Deg2Rad;

        return start + length * new Vector2(
            Mathf.Cos(rad),
            Mathf.Sin(rad)
        );
    }

    // 사다리 끝점이 판정 영역에서 얼마나 벗어났는지 평가
    public static SlipResult Evaluate(
        Vector2 start,
        float angleDeg,
        float length,
        Rect zone,
        SlipSettings settings)
    {
        Vector2 end = GetEndPoint(start, angleDeg, length);
        float outside = DistanceOutside(zone, end);

        // 판정 영역 안이거나 허용 오차 이내라면 안정
        if (outside <= settings.tolerance)
            return Stable();

        // 벗어난 거리에 비례해 미끄러짐 시작 지점 계산
        float t = Mathf.InverseLerp(
            settings.tolerance,
            settings.maxSlipDistance,
            outside
        );

        return Unstable(
            Mathf.Lerp(settings.maxSlipPoint, settings.minSlipPoint, t)
        );
    }

    // 점이 사각형 바깥으로 벗어난 거리 계산
    public static float DistanceOutside(Rect zone, Vector2 point)
    {
        float dx = Mathf.Max(
            zone.xMin - point.x,
            0f,
            point.x - zone.xMax
        );

        float dy = Mathf.Max(
            zone.yMin - point.y,
            0f,
            point.y - zone.yMax
        );

        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    // 안정 판정 결과 반환
    public static SlipResult Stable()
    {
        return new SlipResult
        {
            isStable = true,
            slipPoint = 1f
        };
    }

    // 불안정 판정 결과 반환
    public static SlipResult Unstable(float slipPoint)
    {
        return new SlipResult
        {
            isStable = false,
            slipPoint = Mathf.Clamp01(slipPoint)
        };
    }
}
