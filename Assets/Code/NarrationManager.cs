using UnityEngine;
using TMPro;
using System.Collections;

public class NarrationManager : MonoBehaviour
{
    public GameObject narrationPanel;
    public TMP_Text narrationText;

    private Coroutine hideCoroutine;

    public NarrationData[] slipNarrations;
    public NarrationData[] LowerNarrations;
    public NarrationData[] MiddleNarrations;
    public NarrationData[] UpperNarrations;

    private void Start()
    {
        HideNarration();
    }

    public void ShowNarration(NarrationData data)
    {
        if (data == null) return;

        narrationText.text = data.narrationText;
        narrationPanel.SetActive(true);
    }

    public void ShowNarrationForSeconds(NarrationData data, float seconds)
    {
        if (data == null) return;

        // 기존 자동 종료 예약 취소
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        narrationText.text = data.narrationText;
        narrationPanel.SetActive(true);

        hideCoroutine = StartCoroutine(HideAfterSeconds(seconds));
    }

    private IEnumerator HideAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        HideNarration();
        hideCoroutine = null;
    }

    public void HideNarration()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        narrationPanel.SetActive(false);
    }

    public void ShowSlipNarration()
    {
        ShowNarrationForSeconds(slipNarrations[Random.Range(0,8)], 2f);
    }

    public void ShowLowerNarration(int n)
    {
        ShowNarrationForSeconds(LowerNarrations[n], 5f);
    }
    public void ShowMiddleNarration(int n)
    {
        ShowNarrationForSeconds(MiddleNarrations[n], 5f);
    }
    public void ShowUpperNarration(int n)
    {
        // Upper005(인덱스 4)가 나올 때는 Upper006(인덱스 5)까지 이어서 출력하며 엔딩 연출 시작
        // (줌 인 -> 하얀 페이드 아웃 -> End 씬)
        if (n == 4 && UpperNarrations.Length > 5)
        {
            EndingSequence ending = GetComponent<EndingSequence>();
            if (ending == null)
                ending = gameObject.AddComponent<EndingSequence>();

            ending.Play(this, 6f, UpperNarrations[4], UpperNarrations[5]);
            return;
        }

        ShowNarrationForSeconds(UpperNarrations[n], 5f);
    }

    // 여러 나레이션을 순서대로, 각각 secondsEach초씩 출력한 뒤 패널을 끈다.
    public void ShowNarrationSequence(float seconds, params NarrationData[] narrations)
    {
        if (narrations == null || narrations.Length == 0) return;

        // 기존 자동 종료 예약(또는 진행 중인 연속 출력) 취소
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        // hideCoroutine에 저장해 두면, 도중에 다른 나레이션이 나오거나
        // HideNarration()이 호출될 때 연속 출력도 함께 중단된다
        hideCoroutine = StartCoroutine(NarrationSequence(narrations, seconds));
    }

    private IEnumerator NarrationSequence(NarrationData[] narrations, float seconds)
    {
        foreach (NarrationData data in narrations)
        {
            if (data == null) continue;

            narrationText.text = data.narrationText;
            narrationPanel.SetActive(true);

            yield return new WaitForSeconds(seconds);
        }

        hideCoroutine = null;
        HideNarration();
    }
}