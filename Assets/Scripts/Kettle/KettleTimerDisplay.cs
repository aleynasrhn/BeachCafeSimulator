using UnityEngine;
using TMPro;

public class KettleTimerDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;


    // =========================================================
    // BAŞLANGIÇ
    // =========================================================

    private void Start()
    {
        HideTimer();
    }


    // =========================================================
    // SAYACI GÖSTER
    // =========================================================

    public void ShowTimer(int seconds)
    {
        if (timerText == null)
            return;

        timerText.gameObject.SetActive(true);
        timerText.text = seconds.ToString();
    }


    // =========================================================
    // SAYAÇ GİZLE
    // =========================================================

    public void HideTimer()
    {
        if (timerText == null)
            return;

        timerText.gameObject.SetActive(false);
    }
}