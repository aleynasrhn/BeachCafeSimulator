using System.Collections;
using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    [Header("Para")]
    [SerializeField] private float currentMoney = 0f;

    [Header("Para UI")]
    [SerializeField] private TMP_Text moneyText;

    [Header("Para Değişim Göstergesi")]
    [SerializeField] private TMP_Text moneyDeltaText;
    [SerializeField] private float deltaDisplayDuration = 2f;
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;

    private Coroutine deltaCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (moneyDeltaText != null)
        {
            moneyDeltaText.gameObject.SetActive(false);
        }

        UpdateMoneyText();
    }

    public void AddMoney(float amount)
    {
        currentMoney += amount;

        UpdateMoneyText();

        Debug.Log(
            $"Para eklendi: +{amount:0.00}$ | " +
            $"Toplam Para: {currentMoney:0.00}$"
        );
    }

    public void SubtractMoney(float amount)
    {
        currentMoney -= amount;

        UpdateMoneyText();

        Debug.Log(
            $"Para düştü: -{amount:0.00}$ | " +
            $"Toplam Para: {currentMoney:0.00}$"
        );
    }

    public void AddMoneyWithFeedback(float amount)
    {
        AddMoney(amount);

        ShowMoneyDelta(amount);
    }

    public void SubtractMoneyWithFeedback(float amount)
    {
        SubtractMoney(amount);

        ShowMoneyDelta(-amount);
    }

    // =========================================================
    // PARAYI DOĞRUDAN AYARLA (KAYIT YÜKLERKEN KULLANILIR)
    // =========================================================
    //
    // AddMoney/SubtractMoney'den farklı olarak göstergesiz,
    // doğrudan atama yapar. Sadece GameBootstrap'ın kayıt
    // yüklerken kullanması için.
    //
    // =========================================================

    public void SetMoney(float amount)
    {
        currentMoney = amount;

        UpdateMoneyText();
    }

    private void ShowMoneyDelta(float delta)
    {
        if (moneyDeltaText == null)
            return;

        if (deltaCoroutine != null)
        {
            StopCoroutine(deltaCoroutine);
        }

        deltaCoroutine = StartCoroutine(ShowMoneyDeltaRoutine(delta));
    }

    private IEnumerator ShowMoneyDeltaRoutine(float delta)
    {
        moneyDeltaText.gameObject.SetActive(true);

        moneyDeltaText.color =
            delta >= 0f ? positiveColor : negativeColor;

        moneyDeltaText.text =
            (delta >= 0f ? "+" : "-") +
            Mathf.Abs(delta).ToString("0.00") +
            "$";

        yield return new WaitForSeconds(deltaDisplayDuration);

        moneyDeltaText.gameObject.SetActive(false);

        deltaCoroutine = null;
    }

    private void UpdateMoneyText()
    {
        if (moneyText != null)
        {
            moneyText.text =
                $"{currentMoney:0.00}$";
        }
    }

    public float GetMoney()
    {
        return currentMoney;
    }
}