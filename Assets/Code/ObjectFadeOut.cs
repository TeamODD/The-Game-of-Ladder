// ObjectFadeOut.cs
// 오브젝트가 페이드 아웃으로 사라짐
// 타이틀 화면에서 게임시작을 누르면 글씨 오브젝트들이 페이드 아웃으로 사라지게 하려고 만듦
// Canvas 안의 UI 오브젝트용: CanvasGroup의 alpha를 줄여서 자식까지 함께 사라지게 함

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ObjectFadeOut : MonoBehaviour
{
    public float fadeDuration = 1.5f;  // 페이드아웃까지 걸리는 시간 (1.5초)
    private CanvasGroup canvasGroup;

    void Awake()
    {
        CacheCanvasGroup();
    }

    private void CacheCanvasGroup()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    // 외부에서 이 함수를 호출하면 페이드아웃 시작
    public void StartFadeOut()
    {
        CacheCanvasGroup();
        StopAllCoroutines();  // 중복 호출 시 이전 페이드 중단
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        // 페이드 중에는 클릭되지 않도록 (START 버튼 중복 클릭 방지)
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
            yield return null;  // 다음 프레임까지 대기
        }

        // 완전히 투명해지도록 0으로 고정
        canvasGroup.alpha = 0f;

        //gameObject.SetActive(false);
    }
}
