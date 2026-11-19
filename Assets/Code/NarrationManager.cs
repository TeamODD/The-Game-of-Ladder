
using UnityEngine;
using TMPro;

public class NarrationManager : MonoBehaviour
{
    public GameObject narrationPanel;
    public TMP_Text narrationText;

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

    public void HideNarration()
    {
        narrationPanel.SetActive(false);
    }
}