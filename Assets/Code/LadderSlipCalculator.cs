// LadderSlipCalculator.cs
// 미끄러짐 판정 계산기 (순수 C# 클래스. 오브젝트에 붙이는 용도 아님!)
// 시작점, 각도, 길이, 플랫폼 모서리를 받아 안정적인지, 미끄러지는 지점은 어디인지 계산
using System.Diagnostics.Tracing;
using UnityEngine;

// 계산 결과를 담는 구조체


public static class LadderSlipCalculator
{
    /*
    Evaluate의 매개변수 설명
    - start: 사다리 아래쪽 끝의 월드 좌표(플레이어 발 위치, 회전축)
    - angleDeg: 사다리 각도. +X축(오른쪽) 기준 0° (반시계 방향이 '+'임. 수직으로 선 사다리는 90°)
    - length: 사다리의 월드 단위 실제 길이(스케일 배율 아님)
    - platformEdge: 사다리를 걸쳐야 하는 상층 플랫폼 모서리의 월드 좌표
    */
    public static SlipResult Evaluate(Vector2 start, float angleDeg, float length, Vector2 platformEdge)
    {
        SlipResult result = new SlipResult();

        // 끝점x = 시작점x + L * cosθ
        // 끝점y = 시작점y + L * sinθ
        float rad = angleDeg * Mathf.Deg2Rad;  // 각을 호도법으로 표현
        Vector2 end = start + length * new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));  // 끝점 계산
    
        // end와 platformEdge를 비교해 판정 (나중에 수정)
        result.isStable = true;
        result.slipPoint = 1f;
        return result;
    }
}
