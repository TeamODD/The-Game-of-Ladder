// ScreenFader.cs
// 씬 전환 효과 두 가지:
//  - FadeToScene      : 검은 화면으로 페이드 아웃 -> 씬 로드 -> 페이드 인
//  - CrossFadeToScene : 현재 화면을 캡처해 위에 덮어두고 다음 씬을 바로 로드한 뒤,
//                       캡처 이미지만 서서히 투명하게 만들어 검은 화면 없이 넘어감
// 씬에 따로 배치할 필요 없음: 처음 사용할 때 스스로 생성되고 씬이 바뀌어도 유지된다.

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    private static ScreenFader instance;

    public static ScreenFader Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ScreenFader");
                instance = go.AddComponent<ScreenFader>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    public float fadeOutDuration = 0.6f;     // 화면이 어두워지는 시간(초)
    public float fadeInDuration = 0.6f;      // 화면이 다시 밝아지는 시간(초)
    public float crossFadeDuration = 1.0f;   // 이전 씬이 사라지며 다음 씬이 드러나는 시간(초)
    public Color fadeColor = Color.black;

    private Image fadeImage;          // 검은 화면 페이드용
    private RawImage snapshotImage;   // 크로스페이드용 (이전 씬 캡처 이미지)
    private bool isFading;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        // 모든 UI 위에 그려지는 전체 화면 Canvas 생성
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        gameObject.AddComponent<GraphicRaycaster>();

        snapshotImage = CreateFullScreen<RawImage>("SnapshotImage");
        snapshotImage.enabled = false;

        fadeImage = CreateFullScreen<Image>("FadeImage");
        SetAlpha(0f);
    }

    private T CreateFullScreen<T>(string name) where T : Graphic
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(transform, false);
        T graphic = obj.AddComponent<T>();

        RectTransform rect = graphic.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return graphic;
    }

    // ---------- 검은 화면 페이드 ----------

    public void FadeToScene(string sceneName)
    {
        if (isFading) return;   // 버튼 연타로 여러 번 로드되는 것 방지
        StartCoroutine(FadeRoutine(sceneName));
    }

    private IEnumerator FadeRoutine(string sceneName)
    {
        isFading = true;

        yield return Fade(0f, 1f, fadeOutDuration);

        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        while (!load.isDone)
            yield return null;

        yield return Fade(1f, 0f, fadeInDuration);

        isFading = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            // Time.timeScale = 0 이어도 동작하도록 unscaled 시간 사용
            t += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        SetAlpha(to);
    }

    private void SetAlpha(float a)
    {
        Color c = fadeColor;
        c.a = a;
        fadeImage.color = c;
        fadeImage.raycastTarget = a > 0f;   // 화면이 가려져 있는 동안 버튼 클릭 막기
    }

    // ---------- 크로스페이드 (검은 화면 없음) ----------

    public void CrossFadeToScene(string sceneName)
    {
        if (isFading) return;
        StartCoroutine(CrossFadeRoutine(sceneName));
    }

    private IEnumerator CrossFadeRoutine(string sceneName)
    {
        isFading = true;

        // 1. 현재 프레임이 다 그려진 뒤 화면(UI 포함)을 캡처해서 맨 위에 덮는다
        yield return new WaitForEndOfFrame();
        Texture2D snapshot = ScreenCapture.CaptureScreenshotAsTexture();
        snapshotImage.texture = snapshot;
        snapshotImage.color = Color.white;
        snapshotImage.raycastTarget = true;
        snapshotImage.enabled = true;

        // 2. 캡처 이미지 뒤에서 다음 씬을 로드 (화면에는 여전히 이전 씬이 보임)
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        while (!load.isDone)
            yield return null;

        // 3. 캡처 이미지를 서서히 투명하게 -> 다음 씬이 자연스럽게 드러남
        float t = 0f;
        while (t < crossFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            snapshotImage.color = new Color(1f, 1f, 1f, 1f - t / crossFadeDuration);
            yield return null;
        }

        snapshotImage.enabled = false;
        snapshotImage.raycastTarget = false;
        snapshotImage.texture = null;
        Destroy(snapshot);   // 캡처 텍스처 메모리 해제

        isFading = false;
    }
}
