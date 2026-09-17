using TMPro;
using UnityEngine;

public class OrderItemUI : MonoBehaviour
{
    public TMP_Text coffeeNameText;
    public TMP_Text timerText;

    // =========================================================
    // SİPARİŞ BİLGİSİNİ AYARLA
    // =========================================================

    public void Setup(Order order)
    {
        if (coffeeNameText != null)
        {
            coffeeNameText.text = order.coffeeType.ToString();
        }
    }

    // =========================================================
    // SÜRE GÖSTERİMİNİ GÜNCELLE
    // =========================================================
    //
    // Artık zamanlayıcı bu script içinde tutulmuyor — süre,
    // OrderUI tarafından merkezi olarak yönetiliyor (sırada
    // bekleyen siparişlerin de arka planda sayabilmesi için).
    // Bu metod sadece görsel güncelleme yapar.
    //
    // =========================================================

    public void UpdateTimerDisplay(float remainingTime)
    {
        if (timerText == null)
            return;

        int minutes =
            Mathf.FloorToInt(remainingTime / 60f);

        int seconds =
            Mathf.FloorToInt(remainingTime % 60f);

        timerText.text =
            minutes.ToString("0") + ":" +
            seconds.ToString("00");
    }
}