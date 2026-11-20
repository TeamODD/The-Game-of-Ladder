// SceneChanger.cs
// scene 바꾸기

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // (게임 시작)메인 게임 씬으로 넘어감
    public void StartGame()
    {
        SceneManager.LoadScene("InGame");
    }

    // 타이틀 씬으로 넘어감
    public void GoTitle()
    {
        SceneManager.LoadScene("Title");
    }

    // (게임 끝)종료 씬으로 넘어감
    public void EndGame()
    {
        SceneManager.LoadScene("End");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
