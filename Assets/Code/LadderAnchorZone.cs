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

    GameObject platform;

    [Header("판정 사각형 종류")]
    public ZoneType zoneType = ZoneType.Lower;

    private float lowerWidth = 0.8f;
    private float middleWidth = 0.6f;
    private float upperWidth = 0.3f;

    [Header("오프셋")]
    public Vector2 offset = Vector2.zero;

    private Vector2 size;

    private static readonly List<LadderAnchorZone> allZones =
        new List<LadderAnchorZone>();

    private void OnEnable()
    {
        allZones.Add(this);
        ApplyPreset();
    }

    private void OnDisable()
    {
        allZones.Remove(this);
    }

    // Inspector에서 종류나 프리셋 크기를 변경하면 자동 적용
    private void OnValidate()
    {
        ApplyPreset();
    }

    // 선택한 종류에 맞는 크기 적용
    public void ApplyPreset()
    {
        if (platform == null)
            platform = transform.parent.gameObject;

        BoxCollider2D platformCollider =
            platform.GetComponent<BoxCollider2D>();

        if (platformCollider == null)
            return;

        float platformHeight = platformCollider.bounds.size.y;

        switch (zoneType)
        {
            case ZoneType.Lower:
                size = new Vector2(lowerWidth, platformHeight);
                break;

            case ZoneType.Middle:
                size = new Vector2(middleWidth, platformHeight);
                break;

            case ZoneType.Upper:
                size = new Vector2(upperWidth, platformHeight);
                break;
        }
    }

    public Rect GetWorldRect()
    {
        Vector2 center = (Vector2)transform.position + offset;
        return new Rect(center - size * 0.5f, size);
    }

    public static LadderAnchorZone FindNearest(
        Vector2 point,
        float maxDistance)
    {
        LadderAnchorZone nearest = null;
        float best = maxDistance;

        foreach (LadderAnchorZone zone in allZones)
        {
            float d = New_LadderSlipCalculator.DistanceOutside(
                zone.GetWorldRect(), point);

            if (d <= best)
            {
                best = d;
                nearest = zone;
            }
        }

        return nearest;
    }

    private void OnDrawGizmos()
    {
        Rect r = GetWorldRect();

        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
        Gizmos.DrawCube(r.center, r.size);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(r.center, r.size);
    }
}