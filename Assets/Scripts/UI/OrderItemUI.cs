using TMPro;
using UnityEngine;

public class OrderItemUI : MonoBehaviour
{
    public TMP_Text coffeeNameText;
    public TMP_Text timerText;

    private float remainingTime;

    public void Setup(Order order)
    {
        coffeeNameText.text = order.coffeeType.ToString();

        remainingTime = order.timeLimit;
        UpdateTimerText();
    }

    private void Update()
    {
        if (remainingTime <= 0)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0)
        {
            remainingTime = 0;
            UpdateTimerText();

            // Sipariş sayısını 1 azalt
            OrderUI.Instance.RemoveOrder();

            // Süresi dolan siparişi listeden sil
            Destroy(gameObject);

            return;
        }

        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);

        timerText.text = minutes.ToString("0") + ":" + seconds.ToString("00");
    }
}