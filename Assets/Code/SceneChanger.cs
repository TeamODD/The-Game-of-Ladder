// SceneChanger.cs
// scene 바꾸기 (ScreenFader로 페이드 아웃 -> 페이드 인 하며 전환)

using UnityEngine;

public class SceneChanger : MonoBehaviour
{
    // 메인 게임 씬으로 넘어감
    public void StartGame()
    {
        // 검은 화면 없이 타이틀이 사라지며 게임 씬이 드러남
        ScreenFader.Instance.CrossFadeToScene("InGame");
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
