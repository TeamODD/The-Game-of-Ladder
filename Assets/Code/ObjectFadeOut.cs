// ObjectFadeOut.cs
// 오브젝트가 페이드 아웃으로 사라짐
// 타이틀 화면에서 게임시작을 누르면 글씨 오브젝트들이 페이드 아웃으로 사라지게 하려고 만듦

using System.Collections;
using UnityEngine;

public class ObjectFadeOut : MonoBehaviour
{
    public float fadeDuration = 1.5f;  // 페이드아웃까지 걸리는 시간 (1.5초)
    private Material objMaterial;

    void Start()
    {
        CacheMaterial();
    }

    private void CacheMaterial()
    {
        if (objMaterial == null)
            objMaterial = GetComponent<Renderer>().material;
    }

    // 외부에서 이 함수를 호출하면 페이드아웃 시작
    public void StartFadeOut()
    {
        CacheMaterial();  // Start()보다 먼저 호출돼도 안전하도록
        StopAllCoroutines();  // 중복 호출 시 이전 페이드 중단
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        Color startColor = objMaterial.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / fadeDuration);

            objMaterial.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);
            yield return null;  // 다음 프레임까지 대기
        }

        // 완전히 투명해지도록 0으로 고정
        objMaterial.color = new Color(startColor.r, startColor.g, startColor.b, 0f);

        //gameObject.SetActive(false);
    }
}
