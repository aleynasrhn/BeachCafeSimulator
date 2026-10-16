using UnityEngine;

/// <summary>
/// POS cihazının fiziksel tuşlarına eklenir.
///
/// İki farklı şekilde çalışabilir:
/// 1) Oyuncu normal etkileşim sistemiyle E'ye basabilir.
/// 2) POS ekranındayken mouse ile direkt tuşa tıklayabilir.
///
/// KURULUM:
/// - Her fiziksel POS tuşuna bu scripti ekle.
/// - Her tuşta Collider bulunmalı.
/// - Digit için digitValue kısmına 0-9 arasındaki rakamı yaz.
/// - Decimal = nokta
/// - Backspace = sarı silme tuşu
/// - Clear = kırmızı temizleme tuşu
/// - Confirm = yeşil onaylama tuşu
/// </summary>
public class PosButtonInteractable : MonoBehaviour, IInteractable
{
    public enum PosButtonType
    {
        Digit,
        Decimal,
        Backspace,
        Clear,
        Confirm
    }

    [Header("Buton Türü")]
    [SerializeField]
    private PosButtonType buttonType =
        PosButtonType.Digit;

    [Header("Sadece Digit Türü İçin")]
    [Tooltip("Bu tuşun üzerinde yazan rakam. Örn: 7")]
    [SerializeField]
    private string digitValue = "0";

    [Header("E Etkileşim Mesajı")]
    [SerializeField]
    private string customPrompt = "";


    // =========================================================
    // ETKİLEŞİM MESAJI
    // =========================================================

    public string GetInteractPrompt()
    {
        if (!string.IsNullOrEmpty(customPrompt))
        {
            return customPrompt;
        }

        switch (buttonType)
        {
            case PosButtonType.Digit:
                return $"E - {digitValue}";

            case PosButtonType.Decimal:
                return "E - Virgül";

            case PosButtonType.Backspace:
                return "E - Sil";

            case PosButtonType.Clear:
                return "E - Temizle";

            case PosButtonType.Confirm:
                return "E - Onayla";
        }

        return "E - Etkileşim";
    }


    // =========================================================
    // NORMAL E ETKİLEŞİMİ
    // =========================================================

    public void Interact(PlayerInteraction player)
    {
        ExecuteButton();
    }


    // =========================================================
    // MOUSE TIKLAMASI
    // =========================================================
    //
    // PosMachineController mouse raycast ile bunu çağırır.
    //
    // =========================================================

    public void MouseInteract()
    {
        ExecuteButton();
    }


    // =========================================================
    // BUTONUN ASIL İŞLEMİ
    // =========================================================

    private void ExecuteButton()
    {
        if (PosMachineController.Instance == null)
        {
            Debug.LogWarning(
                "PosButtonInteractable: " +
                "PosMachineController bulunamadı!"
            );

            return;
        }

        switch (buttonType)
        {
            case PosButtonType.Digit:

                PosMachineController.Instance.OnDigitPressed(
                    digitValue
                );

                break;


            case PosButtonType.Decimal:

                PosMachineController.Instance.OnDecimalPressed();

                break;


            case PosButtonType.Backspace:

                PosMachineController.Instance.OnBackspacePressed();

                break;


            case PosButtonType.Clear:

                PosMachineController.Instance.OnClearPressed();

                break;


            case PosButtonType.Confirm:

                PosMachineController.Instance.OnConfirmPressed();

                break;
        }
    }
}