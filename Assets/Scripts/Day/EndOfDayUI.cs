using System.Collections;
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

    [Header("Geçiş Süreleri")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float blackHoldDuration = 2f;
    [SerializeField] private float fadeInDuration = 1f;

    // =========================================================
    // PauseMenuController, gün sonu paneli açıkken ESC'ye
    // basılınca pause menüsünün ÜSTÜNE binmesini engellemek
    // için bunu kontrol eder.
    // =========================================================

    public static bool IsShowing { get; private set; } = false;

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

    private void Start()
    {
        SubscribeToDayManager();
    }

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
    }

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

        IsShowing = false;
    }

    public void Show(DayResult result)
    {
        if (panelRoot == null)
        {
            Debug.LogError(
                "EndOfDayUI: Panel Root atanmadı!"
            );

            return;
        }

        panelRoot.SetActive(true);

        IsShowing = true;

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (dayNumberText != null)
            dayNumberText.text = $"Gün {result.dayNumber}";

        if (workHoursText != null)
            workHoursText.text = result.workHoursText;

        if (totalEarnedText != null)
            totalEarnedText.text = $"{result.totalGrossRevenue:0.00}$";

        if (wrongOrdersText != null)
            wrongOrdersText.text = $"-{result.totalPenalty:0.00}$";

        if (profitText != null)
            profitText.text = $"{result.profit:0.00}$";

        if (totalOrdersText != null)
            totalOrdersText.text = result.totalOrdersPlaced.ToString();

        if (successfulOrdersText != null)
            successfulOrdersText.text = result.successfulOrders.ToString();

        if (failedOrdersText != null)
            failedOrdersText.text = result.failedOrders.ToString();

        if (customersArrivedText != null)
            customersArrivedText.text = result.customersArrived.ToString();

        if (satisfiedText != null)
            satisfiedText.text = result.satisfiedCustomers.ToString();

        if (unsatisfiedText != null)
            unsatisfiedText.text = result.unsatisfiedCustomers.ToString();

        UpdateStars(result.starRating);
    }

    private void UpdateStars(int rating)
    {
        if (starIcons == null)
            return;

        rating = Mathf.Clamp(rating, 0, 5);

        for (int i = 0; i < starIcons.Length; i++)
        {
            if (starIcons[i] == null)
                continue;

            starIcons[i].sprite =
                i < rating ? filledStarSprite : emptyStarSprite;
        }
    }

    // =========================================================
    // DEVAM ET: FADE + SAHNE SIFIRLAMA + YENİ GÜN + OTOMATİK KAYIT
    // =========================================================

    private void OnContinueClicked()
    {
        StartCoroutine(ContinueRoutine());
    }

    private IEnumerator ContinueRoutine()
    {
        if (continueButton != null)
        {
            continueButton.interactable = false;
        }

        if (FadeScreenUI.Instance != null)
        {
            yield return FadeScreenUI.Instance.FadeOut(fadeOutDuration);
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        IsShowing = false;

        DayResetManager.ResetSceneForNewDay();

        yield return new WaitForSecondsRealtime(blackHoldDuration);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.PrepareNextDay();
        }

        GameSaveData saveData = new GameSaveData
        {
            currentDay =
                DayManager.Instance != null
                    ? DayManager.Instance.CurrentDay
                    : 1,

            money =
                MoneyManager.Instance != null
                    ? MoneyManager.Instance.GetMoney()
                    : 0f
        };

        SaveManager.Save(saveData);

        if (FadeScreenUI.Instance != null)
        {
            yield return FadeScreenUI.Instance.FadeIn(fadeInDuration);
        }

        if (continueButton != null)
        {
            continueButton.interactable = true;
        }
    }
}