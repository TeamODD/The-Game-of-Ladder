// SceneChanger.cs
// scene 바꾸기 (ScreenFader로 페이드 아웃 -> 페이드 인 하며 전환)

using System.Collections;
using UnityEngine;

public class SceneChanger : MonoBehaviour
{
    [Tooltip("StartGame() 호출 시 씬 전환 전에 페이드 아웃시킬 오브젝트들")]
    public ObjectFadeOut[] fadeOutObjects;

    private bool isChanging = false;  // 버튼 연타로 중복 전환되는 것 방지

    // 메인 게임 씬으로 넘어감
    public void StartGame()
    {
        if (isChanging) return;
        isChanging = true;

        StartCoroutine(FadeOutObjectsThenChange("Scene_ywrain"));
    }

    // 지정한 오브젝트들을 페이드 아웃시킨 뒤, 모두 사라지면 씬 전환
    private IEnumerator FadeOutObjectsThenChange(string sceneName)
    {
        float waitTime = 0f;

        if (fadeOutObjects != null)
        {
            foreach (ObjectFadeOut obj in fadeOutObjects)
            {
                if (obj == null) continue;

                obj.StartFadeOut();
                waitTime = Mathf.Max(waitTime, obj.fadeDuration);  // 가장 오래 걸리는 오브젝트 기준으로 대기
            }
        }

        yield return new WaitForSeconds(waitTime);

        // 검은 화면 없이 타이틀이 사라지며 게임 씬이 드러남
        ScreenFader.Instance.CrossFadeToScene(sceneName);
    }

    // 타이틀 씬으로 넘어감
    public void GoTitle()
    {
        ScreenFader.Instance.FadeToScene("Title");
    }

    // 종료 씬으로 넘어감
    public void GoEnd()
    {
        ScreenFader.Instance.FadeToScene("End");
    }

    // 프로그램 종료
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
