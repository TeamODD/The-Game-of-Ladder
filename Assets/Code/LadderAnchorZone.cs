// LadderAnchorZone.cs
// 플랫폼 모서리의 사다리 설치 판정용 사각형 (오브젝트에 추가)
using System.Collections.Generic;
using UnityEngine;

public class LadderAnchorZone : MonoBehaviour
{
    [Header("판정 사각형")]
    public Vector2 size = new Vector2(0.4f, 0.3f);  // 수치 바꿔도 됨
    public Vector2 offset = Vector2.zero;

    // 씬에 있는 모든 판정 사각형 목록
    private static readonly List<LadderAnchorZone> allZones = new List<LadderAnchorZone>();

    private void OnEnable()  { allZones.Add(this); }
    private void OnDisable() { allZones.Remove(this); }

    // 월드 좌표 기준 사각형
    public Rect GetWorldRect()
    {
        Vector2 center = (Vector2)transform.position + offset;
        return new Rect(center - size * 0.5f, size);
    }

    // point에서 maxDistance 안에 있는 가장 가까운 판정 사각형 (없으면 null)
    public static LadderAnchorZone FindNearest(Vector2 point, float maxDistance)
    {
        LadderAnchorZone nearest = null;
        float best = maxDistance;

        foreach (LadderAnchorZone zone in allZones)
        {
            float d = New_LadderSlipCalculator.DistanceOutside(zone.GetWorldRect(), point);
            if (d <= best)
            {
                best = d;
                nearest = zone;
            }
        }
        return nearest;
    }

    // 씬 뷰에서만 보이는 사각형 표시
    private void OnDrawGizmos()
    {
        Rect r = GetWorldRect();
        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
        Gizmos.DrawCube(r.center, r.size);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(r.center, r.size);
    }
}
