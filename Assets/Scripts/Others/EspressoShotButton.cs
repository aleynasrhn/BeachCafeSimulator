using UnityEngine;

/// <summary>
/// Espresso makinesindeki fiziksel Single / Double butonunu kontrol eder.
///
/// Seçilen butonun LED'ini yakar ve diğer shot LED'ini söndürür.
///
/// Brew sırasında buton kilitlenebilir.
/// </summary>
public class EspressoShotButton : MonoBehaviour, IInteractable
{
    public enum ShotType
    {
        Single,
        Double
    }


    [Header("Buton Ayarı")]
    [SerializeField] private ShotType shotType;


    [Header("LED")]
    [SerializeField] private GameObject led;


    // Şu anda seçili olan fiziksel shot butonu.
    private static EspressoShotButton selectedButton;


    // Bu buton kilitli mi?
    private bool isLocked = false;


    // =========================================================
    // DIŞARIDAN OKUNANLAR
    // =========================================================

    public ShotType Shot =>
        shotType;


    public bool IsSelected =>
        selectedButton == this;


    public bool IsLocked =>
        isLocked;


    // =========================================================
    // BAŞLANGIÇ
    // =========================================================

    private void Start()
    {
        SetLED(false);
    }


    // =========================================================
    // PROMPT
    // =========================================================

    public string GetInteractPrompt()
    {
        if (isLocked)
            return "Makine çalışıyor...";


        if (IsSelected)
            return $"{shotType} seçili";


        return $"E - {shotType}";
    }


    // =========================================================
    // INTERACT
    // =========================================================

    public void Interact(PlayerInteraction player)
    {
        // Brew sırasında kullanılamaz.
        if (isLocked)
            return;


        SelectShot();
    }


    // =========================================================
    // SHOT SEÇ
    // =========================================================

    public void SelectShot()
    {
        // Güvenlik için.
        if (isLocked)
            return;


        // Daha önce başka bir buton seçildiyse
        // onun LED'ini söndür.
        if (selectedButton != null &&
            selectedButton != this)
        {
            selectedButton.SetSelected(false);
        }


        // Bu butonu seç.
        SetSelected(true);

        selectedButton = this;
    }


    // =========================================================
    // SEÇİM DURUMU
    // =========================================================

    public void SetSelected(bool selected)
    {
        SetLED(selected);


        // Seçim kaldırılan buton kendisiyse
        // ortak seçimi temizle.
        if (!selected &&
            selectedButton == this)
        {
            selectedButton = null;
        }
    }


    // =========================================================
    // KİLİT
    // =========================================================

    /// <summary>
    /// Butonu kilitler veya kilidini açar.
    ///
    /// Kilitliyken:
    /// - Butona basılamaz.
    /// - Shot değiştirilemez.
    /// </summary>
    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }


    // =========================================================
    // LED
    // =========================================================

    private void SetLED(bool state)
    {
        if (led != null)
        {
            led.SetActive(state);
        }
    }


    // =========================================================
    // SEÇİLİ BUTON
    // =========================================================

    public static EspressoShotButton GetSelectedButton()
    {
        return selectedButton;
    }
}