using UnityEngine;

/// <summary>
/// Kullanılmış espresso kahvesini portafilterdan boşaltır.
/// Oyuncu elinde kullanılmış kahveli portafilter tutarken
/// çöp kovasına E ile etkileşirse kahve temizlenir.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CoffeeTrashBin : MonoBehaviour, IInteractable
{
    [Header("Ayarlar")]
    [SerializeField] private string acceptedItemName = "Portafilter";


    public string GetInteractPrompt()
    {
        return "Kahveyi boşalt";
    }


    public void Interact(PlayerInteraction player)
    {
        if (player == null)
            return;


        PickupItem heldItem =
            player.GetHeldItem();


        // Elinde item yok.
        if (heldItem == null)
        {
            Debug.Log("Çöp kovası: Oyuncunun elinde item yok.");
            return;
        }


        Debug.Log(
            "Çöp kovası: Elde olan item = " +
            heldItem.ItemName
        );


        // Portafilter değil.
        if (heldItem.ItemName != acceptedItemName)
        {
            Debug.Log(
                "Çöp kovası: Bu item portafilter değil."
            );

            return;
        }


        // Kullanılmış kahve kontrolü.
        if (!heldItem.HasUsedCoffee)
        {
            Debug.Log(
                "Çöp kovası: Portafilterde kullanılmış kahve yok."
            );

            return;
        }


        Debug.Log(
            "Çöp kovası: Kullanılmış kahve temizleniyor..."
        );


        // KAHVEYİ SİL
        heldItem.EmptyGroundCoffee();


        Debug.Log(
            "Çöp kovası: Kahve başarıyla boşaltıldı."
        );
    }
}