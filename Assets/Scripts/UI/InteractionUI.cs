using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InteractionUI : MonoBehaviour
{
    [Header("Basmadan önceki prompt")]
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TMP_Text promptText;

    [Header("Basılı tutma göstergesi")]
    [SerializeField] private GameObject holdIndicatorRoot;
    [SerializeField] private Image radialFillImage;
    [SerializeField] private TMP_Text secondsText;

    public void ShowPrompt(string text)
    {
        if (promptRoot != null && !promptRoot.activeSelf)
            promptRoot.SetActive(true);

        if (promptText != null)
            promptText.text = text;
    }

    public void HidePrompt()
    {
        if (promptRoot != null && promptRoot.activeSelf)
            promptRoot.SetActive(false);
    }

    public void ShowHoldProgress(float progress01, int secondsRemaining)
    {
        if (holdIndicatorRoot != null && !holdIndicatorRoot.activeSelf)
            holdIndicatorRoot.SetActive(true);

        if (radialFillImage != null)
            radialFillImage.fillAmount = Mathf.Clamp01(progress01);

        if (secondsText != null)
            secondsText.text = secondsRemaining.ToString();
    }

    public void HideHoldProgress()
    {
        if (holdIndicatorRoot != null && holdIndicatorRoot.activeSelf)
            holdIndicatorRoot.SetActive(false);
    }

    public IEnumerator ShowCountdown(float duration)
    {
        if (holdIndicatorRoot != null)
            holdIndicatorRoot.SetActive(true);

        float timer = duration;

        while (timer > 0f)
        {
            if (secondsText != null)
                secondsText.text = Mathf.CeilToInt(timer).ToString();

            if (radialFillImage != null)
                radialFillImage.fillAmount = timer / duration;

            timer -= Time.deltaTime;
            yield return null;
        }

        if (radialFillImage != null)
            radialFillImage.fillAmount = 0;

        if (holdIndicatorRoot != null)
            holdIndicatorRoot.SetActive(false);
    }
}