
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

    private float leftRailBottom;
    private float rightRailBottom;

    private float leftRailBottomOffset;
    private float rightRailBottomOffset;

    private Vector3 basePathPosition;
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
            Debug.LogError(
                "[LadderPartsController] Inspector 참조가 빠져 있어!"
            );
            enabled = false;
            return;
        }

        // 1. 기둥의 기본 크기와 위치 저장
        baseLeftRailSize = leftRail.size.y;
        baseRightRailSize = rightRail.size.y;

        leftRailPosition = leftRail.transform.localPosition;
        rightRailPosition = rightRail.transform.localPosition;

        leftRailBottom = leftRailPosition.y - baseLeftRailSize * 0.5f;
        rightRailBottom = rightRailPosition.y - baseRightRailSize * 0.5f;

        // LadderStart와 기둥 아래쪽 사이의 기존 간격 보존
        float startY = transform.InverseTransformPoint(
            ladderStart.position
        ).y;

        leftRailBottomOffset = leftRailBottom - startY;
        rightRailBottomOffset = rightRailBottom - startY;

        // 2. LadderPath의 기본 위치와 크기 저장
        basePathPosition = transform.InverseTransformPoint(
            ladderPath.position
        );

        basePathScale = ladderPath.localScale;
        basePathHeight = Mathf.Abs(basePathScale.y);

        float pathBottom = basePathPosition.y - basePathHeight * 0.5f;

        pathBottomOffset = pathBottom - startY;

        // LadderCheck와 LadderPath 위쪽 끝 사이의 기존 간격 저장
        Vector3 checkPosition = transform.InverseTransformPoint(
            ladderCheck.position
        );

        float pathTop = pathBottom + basePathHeight;
        checkTopOffset = checkPosition.y - pathTop;

        // 3. 기본 길이 설정
        SetLength(1f);
    }

    public void SetLength(float length)
    {
        currentLength = Mathf.Clamp(length, minLength, maxLength);

        float startY = transform.InverseTransformPoint(
            ladderStart.position
        ).y;

        // =================================
        // 1. 기둥 길이 조절
        // =================================

        Vector2 leftSize = leftRail.size;
        leftSize.y = baseLeftRailSize * currentLength;
        leftRail.size = leftSize;

        Vector2 rightSize = rightRail.size;
        rightSize.y = baseRightRailSize * currentLength;
        rightRail.size = rightSize;

        // 기둥 아래쪽 끝은 기존 위치에 고정
        Vector3 leftPos = leftRailPosition;
        leftPos.y = startY + leftRailBottomOffset
                    + leftSize.y * 0.5f;

        leftRail.transform.localPosition = leftPos;

        Vector3 rightPos = rightRailPosition;
        rightPos.y = startY + rightRailBottomOffset
                     + rightSize.y * 0.5f;

        rightRail.transform.localPosition = rightPos;

        // =================================
        // 2. 사다릿대 생성 및 배치
        // =================================

        int rungCount = Mathf.Max(
            1,
            Mathf.RoundToInt(currentLength * rungsAtBaseLength)
        );

        // 필요한 개수만큼 생성
        while (rungs.Count < rungCount)
        {
            GameObject rung = Instantiate(
                rungPrefab,
                rungContainer
            );

            rungs.Add(rung.transform);
        }

        float totalHeight = baseLeftRailSize * currentLength;

        float rungBottom = startY + leftRailBottomOffset;
        float spacing = totalHeight / (rungCount + 1);

        // 두 기둥의 중간 X 좌표
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

            Vector3 ladderLocalPosition = new Vector3(
                centerX,
                y,
                0f
            );

            Vector3 worldPosition = transform.TransformPoint(
                ladderLocalPosition
            );

            rungs[i].localPosition =
                rungContainer.InverseTransformPoint(worldPosition);
        }

        // =================================
        // 3. LadderPath 길이 및 위치 조절
        // =================================

        // LadderPath: 위치는 유지하고 길이만 조절
        float newPathHeight = basePathHeight + baseLeftRailSize * (currentLength - 1f);
        float newPathBottom = startY + pathBottomOffset;

        Vector3 newPathScale = basePathScale;
        newPathScale.y = basePathScale.y * (newPathHeight / basePathHeight);

        ladderPath.localScale = newPathScale;

        // =================================
        // 4. LadderCheck 위치 조절
        // =================================

        Vector3 checkLocalPosition = transform.InverseTransformPoint(
            ladderCheck.position
        );

        float newPathTop = newPathBottom + newPathHeight;

        checkLocalPosition.y = newPathTop + checkTopOffset;

        ladderCheck.position = transform.TransformPoint(
            checkLocalPosition
        );
    }

        public void AdjustLength(float input, float deltaTime)
    {
        float nextLength = currentLength
            + input * lengthChangeSpeed * deltaTime;

        SetLength(nextLength);
    }

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
