using UnityEngine;

/// <summary>
/// Coffee grinder objesine eklenir. Oyuncu elinde BOŞ portafilter varken buraya
/// bakıp E'yi holdDuration kadar basılı tutunca içi kahveyle dolar.
/// Zaten dolu bir portafilterle tekrar denerse hiçbir şey olmaz (CanStartHold false döner).
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
        if (held == null) return false;                      // elinde bir şey yok
        if (held.ItemName != acceptedItemName) return false;  // portafilter değil
        if (held.HasGroundCoffee) return false;                // zaten dolu, tekrar öğütülemez
        return true;
    }

    public void OnHoldComplete(PlayerInteraction player)
    {
        PickupItem held = player.GetHeldItem();
        if (held == null) return;

        held.FillWithGroundCoffee();
    }
}