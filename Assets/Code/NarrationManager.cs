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
        ShowNarrationForSeconds(UpperNarrations[n], 5f);
    }
}