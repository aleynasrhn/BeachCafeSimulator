using UnityEngine;

/// <summary>
/// İki farklı işi yapar:
/// 1) Elinde kullanılmış kahveli PORTAFİLTER varsa: sadece kahveyi
///    boşaltır (portafilter elde kalır, tekrar kullanılabilir).
/// 2) Elinde (masadan alınmış) herhangi bir KAHVE BARDAĞI (DrinkRecipe
///    içeren item) varsa: bardağı tamamen yok eder, elden düşer.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CoffeeTrashBin : MonoBehaviour, IInteractable
{
    [Header("Ayarlar")]
    [SerializeField] private string portafilterItemName = "Portafilter";


    public string GetInteractPrompt()
    {
        return "E - Çöp Kovası";
    }


    public void Interact(PlayerInteraction player)
    {
        if (player == null)
            return;


        bool fromLeftHand;

        PickupItem heldItem =
            GetHeldFrom(player, out fromLeftHand);


        if (heldItem == null)
        {
            Debug.Log("Çöp kovası: Oyuncunun elinde item yok.");
            return;
        }


        // =====================================================
        // DURUM 1: PORTAFİLTER → SADECE KAHVEYİ BOŞALT
        // =====================================================

        if (heldItem.ItemName == portafilterItemName)
        {
            if (!heldItem.HasUsedCoffee)
            {
                Debug.Log(
                    "Çöp kovası: Portafilterde kullanılmış kahve yok."
                );

                return;
            }

            heldItem.EmptyGroundCoffee();

            Debug.Log(
                "Çöp kovası: Kullanılmış kahve boşaltıldı."
            );

            return;
        }


        // =====================================================
        // DURUM 2: KAHVE BARDAĞI → TAMAMEN AT
        // =====================================================

        DrinkRecipe recipe =
            heldItem.GetComponent<DrinkRecipe>();

        if (recipe != null)
        {
            if (fromLeftHand)
            {
                player.SetLeftHeldItem(null);
            }
            else
            {
                player.SetHeldItem(null);
            }

            Destroy(heldItem.gameObject);

            Debug.Log("Çöp kovası: Bardak atıldı.");

            return;
        }


        // =====================================================
        // NE PORTAFİLTER NE BARDAK
        // =====================================================

        Debug.Log(
            "Çöp kovası: Bu item kabul edilmiyor " +
            "(ne portafilter ne de bardak)."
        );
    }


    // =========================================================
    // SAĞ YA DA SOL ELDEKİ ITEM'I BUL
    // =========================================================

    private PickupItem GetHeldFrom(
        PlayerInteraction player,
        out bool fromLeftHand)
    {
        fromLeftHand = false;

        PickupItem held =
            player.GetHeldItem();

        if (held != null)
            return held;

        held =
            player.GetLeftHeldItem();

        if (held != null)
        {
            fromLeftHand = true;
            return held;
        }

        return null;
    }
}