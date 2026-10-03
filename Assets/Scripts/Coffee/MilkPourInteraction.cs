using UnityEngine;

/// <summary>
/// Milk pitcher/frother objesine, PickupItem ve MilkFiller'ın YANINA eklenir.
/// Oyuncu elinde süt (Milk) varken pitcher'a bakıp E'yi basılı tutunca,
/// MilkFiller'daki dökülme animasyonu (StartPouring) tetiklenir - tıpkı
/// grinder'daki kahve öğütme mekaniği gibi.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MilkPourInteraction : MonoBehaviour, IHoldInteractable
{
    [SerializeField] private float holdDuration = 2f;
    [SerializeField] private string acceptedItemName = "Milk";
    [Tooltip("Aynı objedeki (ya da başka bir yerdeki) MilkFiller component'ini buraya sürükle")]
    [SerializeField] private MilkFiller milkFiller;

    public float HoldDuration => holdDuration;

    public string GetHoldPrompt()
    {
        return "E'ye basılı tut";
    }

    public bool CanStartHold(PlayerInteraction player)
    {
        PickupItem held = player.GetHeldItem();
        if (held == null) return false;
        if (held.ItemName != acceptedItemName) return false;
        if (milkFiller == null) return false;
        if (milkFiller.IsFull) return false; // sadece "zaten tam dolu mu" kontrolü - devam eden dolumu engellemez
        return true;
    }

    public void OnHoldProgress(PlayerInteraction player, float progress01)
    {
        milkFiller.SetFillProgress(progress01); // E basılı tutuldukça CANLI dolar
    }

    public void OnHoldComplete(PlayerInteraction player)
    {
        milkFiller.SetFillProgress(1f);
        milkFiller.CompleteFill();
    }
}