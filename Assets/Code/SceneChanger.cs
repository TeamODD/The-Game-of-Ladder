// SceneChanger.cs
// scene 바꾸기 (ScreenFader로 페이드 아웃 -> 페이드 인 하며 전환)

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // START 버튼을 눌렀을 때 동시에 페이드 아웃시킬 오브젝트들 (Inspector에서 지정)
    [SerializeField] private ObjectFadeOut[] fadeOutTargets;

    // START 버튼을 눌렀을 때 끌 마우스 따라가기 카메라 (비워두면 씬에서 자동으로 찾음)
    [SerializeField] private MouseFollowCamera mouseFollowCamera;

    // START 버튼을 누른 직후 재생할 소리 (Inspector에서 mp3 지정)
    [SerializeField] private AudioClip descendSound;
    [SerializeField, Range(0f, 1f)] private float descendSoundVolume = 1f;

    // 메인 게임 씬으로 넘어감 (오브젝트들 페이드 아웃 후 1.5초 뒤)
    public void StartGame()
    {
        // START 버튼을 누른 직후 소리 재생
        PlayDescendSound();

        foreach (ObjectFadeOut target in fadeOutTargets)
        {
            if (target != null)
                target.StartFadeOut();
        }

        // 마우스 따라가기를 끄고 카메라를 목표 위치로 부드럽게 이동
        if (mouseFollowCamera == null)
            mouseFollowCamera = FindAnyObjectByType<MouseFollowCamera>();

        if (mouseFollowCamera != null)
        {
            mouseFollowCamera.enabled = false;
            StartCoroutine(CameraSequenceRoutine(mouseFollowCamera.transform));
        }

        StartCoroutine(StartGameRoutine());
    }

    // 카메라 이동 순서: (32.31, 277.2)로 이동 -> 1.7초 후 그 자리에서 (32.31, 10.165)로 이동
    private IEnumerator CameraSequenceRoutine(Transform cam)
    {
        Coroutine firstMove = StartCoroutine(MoveCameraRoutine(cam, new Vector2(32.31f, 277.2f), 0.8f));

        yield return new WaitForSeconds(2f);

        // 첫 번째 이동이 아직 진행 중이면 멈춰야 두 이동이 서로 끌어당기지 않음
        StopCoroutine(firstMove);

        // 아래로 이동하면서 카메라 크기도 15 -> 10으로 변경
        yield return MoveCameraRoutine(cam, new Vector2(32.31f, 10.165f), 1f, 10f);
    }

    private void PlayDescendSound()
    {
        if (descendSound == null)
        {
            Debug.LogWarning("SceneChanger: Descend Sound가 비어 있어서 소리를 재생하지 않습니다.");
            return;
        }

        // 이 오브젝트에 AudioSource가 없으면 자동으로 추가
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        audioSource.clip = descendSound;
        audioSource.volume = descendSoundVolume;
        audioSource.Play();
    }

    // targetSize를 주면 이동한 비율만큼 카메라 크기(Orthographic Size)도 함께 바뀜
    // -> 이동이 끝나는 순간 크기도 정확히 targetSize가 됨
    private IEnumerator MoveCameraRoutine(Transform cam, Vector2 targetPosition, float smoothTime, float? targetSize = null)
    {
        // z는 그대로 유지 (2D 카메라는 z가 바뀌면 화면이 안 보일 수 있음)
        Vector3 target = new Vector3(targetPosition.x, targetPosition.y, cam.position.z);
        Vector3 velocity = Vector3.zero;

        Camera camera = targetSize.HasValue ? cam.GetComponent<Camera>() : null;
        float startSize = camera != null ? camera.orthographicSize : 0f;
        float startDistance = Vector3.Distance(cam.position, target);

        while ((cam.position - target).sqrMagnitude > 0.0001f)
        {
            cam.position = Vector3.SmoothDamp(cam.position, target, ref velocity, smoothTime);

            if (camera != null && startDistance > 0f)
            {
                // 진행도: 출발 0 -> 도착 1
                float progress = 1f - Vector3.Distance(cam.position, target) / startDistance;
                camera.orthographicSize = Mathf.Lerp(startSize, targetSize.Value, progress);
            }

            yield return null;
        }

        cam.position = target;
        if (camera != null)
            camera.orthographicSize = targetSize.Value;
    }

    private IEnumerator StartGameRoutine()
    {
        yield return new WaitForSeconds(8.6f);
        SceneManager.LoadScene("InGame");
    }

    // 타이틀 씬으로 넘어감
    public void GoTitle()
    {
        SceneManager.LoadScene("Title");
    }

    // 종료 씬으로 넘어감
    public void GoEnd()
    {
        SceneManager.LoadScene("End");
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
