using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    [Header("Para")]
    [SerializeField] private float currentMoney = 0f;

    [Header("Para UI")]
    [SerializeField] private TMP_Text moneyText;

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

    private void UpdateMoneyText()
    {
        if (moneyText != null)
        {
            moneyText.text = $"{currentMoney:0.00}$";
        }
    }

    public float GetMoney()
    {
        return currentMoney;
    }
}