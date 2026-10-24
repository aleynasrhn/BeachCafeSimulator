using UnityEngine;

/// <summary>
/// Dışarıdaki çöp konteyneri. Sadece DOLU çöp poşetlerini kabul eder.
/// Kapasite sınırı yok.
/// </summary>
[RequireComponent(typeof(Collider))]
public class OutsideDumpster : MonoBehaviour, IInteractable
{
    [Header("Ekran Mesajı")]
    [SerializeField] private InteractionUI interactionUI;

    public string GetInteractPrompt()
    {
        return "E - Çöp poşetini at";
    }

    public void Interact(PlayerInteraction player)
    {
        if (player == null)
            return;

        bool fromLeftHand = false;

        PickupItem held = player.GetHeldItem();

        if (held == null)
        {
            held = player.GetLeftHeldItem();

            fromLeftHand = true;
        }

        if (held == null)
        {
            Say("Elinde atılacak bir çöp poşeti yok.");

            return;
        }

        TrashBagItem bag =
            held.GetComponent<TrashBagItem>();

        if (bag == null ||
            bag.State != TrashBagItem.BagState.Full)
        {
            Say("Buraya sadece dolu çöp poşetleri atılır.");

            return;
        }

        if (fromLeftHand)
        {
            player.SetLeftHeldItem(null);
        }
        else
        {
            player.SetHeldItem(null);
        }

        Destroy(held.gameObject);

        Say("Çöp atıldı.");
    }

    private void Say(string message)
    {
        if (interactionUI == null)
        {
            interactionUI =
                FindFirstObjectByType<InteractionUI>();
        }

        interactionUI?.ShowMessage(message);
    }
}