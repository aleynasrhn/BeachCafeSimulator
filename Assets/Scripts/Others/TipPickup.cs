using UnityEngine;

/// <summary>
/// Doğru sipariş sonrası masada beliren bahşiş objesi.
/// Oyuncu E ile etkileşince para eklenir ve obje yok olur.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TipPickup : MonoBehaviour, IInteractable
{
    private float tipAmount;
    private DrinkPlacePoint ownerPoint;

    // =========================================================
    // KURULUM
    // =========================================================

    public void Initialize(float amount, DrinkPlacePoint owner)
    {
        tipAmount = amount;
        ownerPoint = owner;
    }

    // =========================================================
    // ETKİLEŞİM
    // =========================================================

    public string GetInteractPrompt()
    {
        return $"E - Bahşişi Al (+{tipAmount:0.00}$)";
    }

    public void Interact(PlayerInteraction player)
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoneyWithFeedback(tipAmount);
        }

        if (ownerPoint != null)
        {
            ownerPoint.ClearTip();
        }

        Debug.Log($"Bahşiş toplandı: +{tipAmount:0.00}$");

        Destroy(gameObject);
    }
}