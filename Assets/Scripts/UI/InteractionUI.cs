using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// İki ayrı UI parçasını yönetir:
/// 1) PromptText - "E'ye basılı tut" yazısı (E'ye basmadan önce, hedef uygunken görünür)
/// 2) HoldIndicator + RadialFill + saniye text'i - E basılıyken dolan halka
/// TextMeshPro (TMP_Text) kullanıyor - legacy UI.Text DEĞİL.
/// </summary>
public class InteractionUI : MonoBehaviour
{
    [Header("Basmadan önceki prompt")]
    [SerializeField] private GameObject promptRoot;   // örn: "PromptText" GameObject
    [SerializeField] private TMP_Text promptText;     // içindeki TextMeshPro component'i

    [Header("Basılı tutma göstergesi")]
    [SerializeField] private GameObject holdIndicatorRoot; // HoldIndicator
    [SerializeField] private Image radialFillImage;         // RadialFill (Filled/Radial360)
    [SerializeField] private TMP_Text secondsText;          // ortadaki "3" "2" "1" yazısı

    public void ShowPrompt(string text)
    {
        if (promptRoot != null && !promptRoot.activeSelf) promptRoot.SetActive(true);
        if (promptText != null) promptText.text = text;
    }

    public void HidePrompt()
    {
        if (promptRoot != null && promptRoot.activeSelf) promptRoot.SetActive(false);
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
}