using UnityEngine;

/// <summary>
/// Coffee grinder objesine eklenir. Oyuncu elinde BOŞ portafilter varken buraya
/// bakıp E'yi holdDuration kadar basılı tutunca içi kahveyle dolar.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CoffeeGrinderInteraction : MonoBehaviour, IHoldInteractable
{
    [SerializeField] private float holdDuration = 3.5f;
    [SerializeField] private string acceptedItemName = "portafilter";

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
        if (held.HasGroundCoffee) return false;
        return true;
    }

    public void OnHoldProgress(PlayerInteraction player, float progress01)
    {
        // Grinder'da animasyon gerekmiyor
    }

    public void OnHoldComplete(PlayerInteraction player)
    {
        PickupItem held = player.GetHeldItem();
        if (held == null) return;

        held.FillWithGroundCoffee();
    }
}