using UnityEngine;

// 크레딧 오브젝트를 시작 위치에서 끝 위치까지 일정한 속도로 이동시킨다.
// UI(RectTransform)면 anchoredPosition을, 일반 오브젝트면 localPosition을 사용한다.
public class ScrollCredit : MonoBehaviour
{
    [SerializeField] private Vector2 startPosition = new Vector2(0f, -655f);
    [SerializeField] private Vector2 endPosition = new Vector2(0f, 372f);

    [Tooltip("초당 이동 거리 (단위/초)")]
    [SerializeField] private float speed = 300f;

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        SetPosition(startPosition);
    }

    void Update()
    {
        Vector2 current = GetPosition();
        if (current == endPosition) return;

        // MoveTowards: 가감속 없이 매 프레임 같은 속도로 이동
        SetPosition(Vector2.MoveTowards(current, endPosition, speed * Time.deltaTime));
    }

    private Vector2 GetPosition()
    {
        if (rectTransform != null) return rectTransform.anchoredPosition;
        return transform.localPosition;
    }

    private void SetPosition(Vector2 position)
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = position;
        }
        else
        {
            transform.localPosition = new Vector3(position.x, position.y, transform.localPosition.z);
        }
    }
}
