
using System.Collections.Generic;
using UnityEngine;

public class LadderPartsController : MonoBehaviour
{
    [Header("기둥")]
    public SpriteRenderer leftRail;
    public SpriteRenderer rightRail;

    [Header("사다릿대")]
    public Transform rungContainer;
    public GameObject rungPrefab;

    [Header("사다리 경로")]
    public Transform ladderStart;
    public Transform ladderPath;
    public Transform ladderCheck;

    [Header("길이 설정")]
    public float minLength = 0.5f;
    public float maxLength = 2.5f;
    public float lengthChangeSpeed = 1f;
    public int rungsAtBaseLength = 10;

    [Header("회전 설정")]
    public float rotateSpeed = 100f;
    public float minRotation = -90f;
    public float maxRotation = 90f;

    private float baseLeftRailSize;
    private float baseRightRailSize;

    private Vector3 leftRailPosition;
    private Vector3 rightRailPosition;

    private float leftRailBottomOffset;
    private float rightRailBottomOffset;

    private Vector3 basePathScale;
    private float basePathHeight;
    private float pathBottomOffset;
    private float checkTopOffset;

    private readonly List<Transform> rungs = new List<Transform>();

    private float currentLength = 1f;

    public float CurrentLength => currentLength;

    private void Awake()
    {
        if (leftRail == null || rightRail == null ||
            rungContainer == null || rungPrefab == null ||
            ladderStart == null || ladderPath == null ||
            ladderCheck == null)
        {
            Debug.LogError("[LadderPartsController] Inspector 참조가 빠져 있어!");
            enabled = false;
            return;
        }

        // 기둥 기본 크기와 위치 저장
        baseLeftRailSize = leftRail.size.y;
        baseRightRailSize = rightRail.size.y;

        leftRailPosition = leftRail.transform.localPosition;
        rightRailPosition = rightRail.transform.localPosition;

        float leftRailBottom =
            leftRailPosition.y - baseLeftRailSize * 0.5f;

        float rightRailBottom =
            rightRailPosition.y - baseRightRailSize * 0.5f;

        float startY = transform.InverseTransformPoint(
            ladderStart.position
        ).y;

        leftRailBottomOffset = leftRailBottom - startY;
        rightRailBottomOffset = rightRailBottom - startY;

        // LadderPath 기본 크기와 위치 저장
        Vector3 basePathPosition = transform.InverseTransformPoint(
            ladderPath.position
        );

        basePathScale = ladderPath.localScale;
        basePathHeight = Mathf.Abs(basePathScale.y);

        float pathBottom = basePathPosition.y - basePathHeight * 0.5f;
        pathBottomOffset = pathBottom - startY;

        // LadderCheck와 LadderPath 상단 사이의 간격 저장
        Vector3 checkPosition = transform.InverseTransformPoint(
            ladderCheck.position
        );

        float pathTop = pathBottom + basePathHeight;
        checkTopOffset = checkPosition.y - pathTop;

        SetLength(1f);
    }

    public void SetLength(float length)
    {
        currentLength = Mathf.Clamp(length, minLength, maxLength);

        float startY = transform.InverseTransformPoint(
            ladderStart.position
        ).y;

        // 기둥 길이 조절
        Vector2 leftSize = leftRail.size;
        leftSize.y = baseLeftRailSize * currentLength;
        leftRail.size = leftSize;

        Vector2 rightSize = rightRail.size;
        rightSize.y = baseRightRailSize * currentLength;
        rightRail.size = rightSize;

        // 기둥 아래쪽 끝 고정
        Vector3 leftPos = leftRailPosition;
        leftPos.y = startY + leftRailBottomOffset + leftSize.y * 0.5f;
        leftRail.transform.localPosition = leftPos;

        Vector3 rightPos = rightRailPosition;
        rightPos.y = startY + rightRailBottomOffset + rightSize.y * 0.5f;
        rightRail.transform.localPosition = rightPos;

        // 사다릿대 생성 및 배치
        int rungCount = Mathf.Max(
            1,
            Mathf.RoundToInt(currentLength * rungsAtBaseLength)
        );

        while (rungs.Count < rungCount)
        {
            GameObject rung = Instantiate(rungPrefab, rungContainer);
            rungs.Add(rung.transform);
        }

        float totalHeight = baseLeftRailSize * currentLength;
        float rungBottom = startY + leftRailBottomOffset;
        float spacing = totalHeight / (rungCount + 1);

        float leftX = leftRail.transform.localPosition.x;
        float rightX = rightRail.transform.localPosition.x;
        float centerX = (leftX + rightX) * 0.5f;

        for (int i = 0; i < rungs.Count; i++)
        {
            bool active = i < rungCount;
            rungs[i].gameObject.SetActive(active);

            if (!active)
                continue;

            float y = rungBottom + spacing * (i + 1);

            Vector3 ladderLocalPosition = new Vector3(centerX, y, 0f);
            Vector3 worldPosition = transform.TransformPoint(ladderLocalPosition);

            rungs[i].localPosition =
                rungContainer.InverseTransformPoint(worldPosition);
        }

        // LadderPath 길이와 위치 조절
        float newPathHeight =
            basePathHeight + baseLeftRailSize * (currentLength - 1f);

        float newPathBottom = startY + pathBottomOffset;

        Vector3 newPathScale = basePathScale;
        newPathScale.y = basePathScale.y * (newPathHeight / basePathHeight);

        ladderPath.localScale = newPathScale;

        // LadderCheck 위치 조절
        Vector3 checkLocalPosition = transform.InverseTransformPoint(
            ladderCheck.position
        );

        float newPathTop = newPathBottom + newPathHeight;
        checkLocalPosition.y = newPathTop + checkTopOffset;

        ladderCheck.position = transform.TransformPoint(checkLocalPosition);
    }

    // 사다리 길이 조절
    public void AdjustLength(float input, float deltaTime)
    {
        float nextLength = currentLength +
            input * lengthChangeSpeed * deltaTime;

        SetLength(nextLength);
    }

    // 사다리 회전 조절
    public void AdjustRotation(Transform target, float input, float deltaTime)
    {
        if (target == null)
            return;

        float angle = target.localEulerAngles.z;

        if (angle > 180f)
            angle -= 360f;

        angle += input * rotateSpeed * deltaTime;
        angle = Mathf.Clamp(angle, minRotation, maxRotation);

        target.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
