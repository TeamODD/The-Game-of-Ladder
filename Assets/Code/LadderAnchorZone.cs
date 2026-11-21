
using System.Collections.Generic;
using UnityEngine;

public class LadderAnchorZone : MonoBehaviour
{
    public enum ZoneType
    {
        Lower,
        Middle,
        Upper
    }

    [Header("판정 사각형 종류")]
    public ZoneType zoneType = ZoneType.Lower;

    [Header("오프셋")]
    public Vector2 offset = Vector2.zero;

    private GameObject platform;
    private Vector2 size;

    private const float LowerWidth = 0.4f;
    private const float MiddleWidth = 0.3f;
    private const float UpperWidth = 0.2f;

    private static readonly List<LadderAnchorZone> allZones =
        new List<LadderAnchorZone>();

    private void OnEnable()
    {
        if (!allZones.Contains(this))
            allZones.Add(this);

        ApplyPreset();
    }

    private void OnDisable()
    {
        allZones.Remove(this);
    }

    // Inspector에서 설정이 변경되면 크기 갱신
    private void OnValidate()
    {
        ApplyPreset();
    }

    // 판정 영역의 크기 설정
    public void ApplyPreset()
    {
        if (transform.parent == null)
            return;

        platform = transform.parent.gameObject;

        BoxCollider2D platformCollider =
            platform.GetComponent<BoxCollider2D>();

        if (platformCollider == null)
            return;

        float platformHeight = platformCollider.bounds.size.y / 2;

        float width = zoneType switch
        {
            ZoneType.Lower => LowerWidth,
            ZoneType.Middle => MiddleWidth,
            ZoneType.Upper => UpperWidth,
            _ => LowerWidth
        };

        size = new Vector2(width, platformHeight);
    }

    // 월드 좌표 기준 판정 사각형 반환
    public Rect GetWorldRect()
    {
        Vector2 center = (Vector2)transform.position + offset;
        return new Rect(center - size * 0.5f, size);
    }

    // 지정한 위치에서 가장 가까운 판정 영역 검색
    public static LadderAnchorZone FindNearest(
        Vector2 point,
        float maxDistance)
    {
        LadderAnchorZone nearest = null;
        float bestDistance = maxDistance;

        foreach (LadderAnchorZone zone in allZones)
        {
            if (zone == null)
                continue;

            float distance = New_LadderSlipCalculator.DistanceOutside(
                zone.GetWorldRect(),
                point
            );

            if (distance <= bestDistance)
            {
                bestDistance = distance;
                nearest = zone;
            }
        }

        return nearest;
    }

    private void OnDrawGizmos()
    {
        Rect rect = GetWorldRect();

        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
        Gizmos.DrawCube(rect.center, rect.size);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(rect.center, rect.size);
    }
}
