using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndOfDayUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;

    [Header("Başlık")]
    [SerializeField] private TMP_Text dayNumberText;
    [SerializeField] private TMP_Text workHoursText;

    [Header("Gelir")]
    [SerializeField] private TMP_Text totalEarnedText;
    [SerializeField] private TMP_Text wrongOrdersText;
    [SerializeField] private TMP_Text profitText;

    [Header("Siparişler")]
    [SerializeField] private TMP_Text totalOrdersText;
    [SerializeField] private TMP_Text successfulOrdersText;
    [SerializeField] private TMP_Text failedOrdersText;

    [Header("Müşteriler")]
    [SerializeField] private TMP_Text customersArrivedText;
    [SerializeField] private TMP_Text satisfiedText;
    [SerializeField] private TMP_Text unsatisfiedText;

    [Header("Yıldızlar (Soldan Sağa Sırayla)")]
    [SerializeField] private Image[] starIcons;
    [SerializeField] private Sprite filledStarSprite;
    [SerializeField] private Sprite emptyStarSprite;

    [Header("Devam Butonu")]
    [SerializeField] private Button continueButton;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
            continueButton.onClick.AddListener(OnContinueClicked);
        }
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        SubscribeToDayManager();
    }

    // =========================================================
    // DAY MANAGER'A BAĞLAN
    // =========================================================

    private void SubscribeToDayManager()
    {
        if (DayCycleManager.Instance == null)
        {
            Debug.LogError(
                "EndOfDayUI: DayCycleManager.Instance bulunamadı!"
            );

            return;
        }

        DayCycleManager.Instance.OnDayEnded -= Show;
        DayCycleManager.Instance.OnDayEnded += Show;

        Debug.Log(
            "EndOfDayUI: OnDayEnded eventine başarıyla bağlandı."
        );
    }

    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnDayEnded -= Show;
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }
    }

    // =========================================================
    // GÜN SONU EKRANINI GÖSTER
    // =========================================================

    public void Show(DayResult result)
    {
        Debug.Log(
            "EndOfDayUI.Show() ÇALIŞTI!"
        );

        if (panelRoot == null)
        {
            Debug.LogError(
                "EndOfDayUI: Panel Root atanmadı!"
            );

            return;
        }

        panelRoot.SetActive(true);

        Time.timeScale = 0f;

        // -----------------------------------------------------
        // BAŞLIK
        // -----------------------------------------------------

        if (dayNumberText != null)
        {
            dayNumberText.text =
                $"Gün {result.dayNumber}";
        }

        if (workHoursText != null)
        {
            workHoursText.text =
                result.workHoursText;
        }

        // -----------------------------------------------------
        // GELİR
        // -----------------------------------------------------

        if (totalEarnedText != null)
        {
            totalEarnedText.text =
                $"{result.totalGrossRevenue:0.00}$";
        }

        if (wrongOrdersText != null)
        {
            wrongOrdersText.text =
                $"-{result.totalPenalty:0.00}$";
        }

        if (profitText != null)
        {
            profitText.text =
                $"{result.profit:0.00}$";
        }

        // -----------------------------------------------------
        // SİPARİŞLER
        // -----------------------------------------------------

        if (totalOrdersText != null)
        {
            totalOrdersText.text =
                result.totalOrdersPlaced.ToString();
        }

        if (successfulOrdersText != null)
        {
            successfulOrdersText.text =
                result.successfulOrders.ToString();
        }

        if (failedOrdersText != null)
        {
            failedOrdersText.text =
                result.failedOrders.ToString();
        }

        // -----------------------------------------------------
        // MÜŞTERİLER
        // -----------------------------------------------------

        if (customersArrivedText != null)
        {
            customersArrivedText.text =
                result.customersArrived.ToString();
        }

        if (satisfiedText != null)
        {
            satisfiedText.text =
                result.satisfiedCustomers.ToString();
        }

        if (unsatisfiedText != null)
        {
            unsatisfiedText.text =
                result.unsatisfiedCustomers.ToString();
        }

        // -----------------------------------------------------
        // YILDIZLAR
        // -----------------------------------------------------

        UpdateStars(
            result.starRating
        );

        Debug.Log(
            $"GÜN SONU UI AÇILDI | " +
            $"Gün: {result.dayNumber} | " +
            $"Gelir: {result.totalGrossRevenue:0.00}$ | " +
            $"Ceza: {result.totalPenalty:0.00}$ | " +
            $"Kâr: {result.profit:0.00}$ | " +
            $"Yıldız: {result.starRating}"
        );
    }

    // =========================================================
    // YILDIZLARI GÜNCELLE
    // =========================================================

    private void UpdateStars(int rating)
    {
        if (starIcons == null)
            return;

        rating =
            Mathf.Clamp(
                rating,
                0,
                5
            );

        for (int i = 0; i < starIcons.Length; i++)
        {
            if (starIcons[i] == null)
                continue;

            if (i < rating)
            {
                starIcons[i].sprite =
                    filledStarSprite;
            }
            else
            {
                starIcons[i].sprite =
                    emptyStarSprite;
            }
        }
    }

    // =========================================================
    // DEVAM ET
    // =========================================================

    private void OnContinueClicked()
    {
        Time.timeScale = 1f;

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.PrepareNextDay();
        }
    }
}