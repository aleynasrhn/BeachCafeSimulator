using UnityEngine;

/// <summary>
/// Çöp kovası. Oyuncunun elinde LitterMarker'lı bir PickupItem
/// varsa kabul eder. Hangi bölgeden geldiği önemli değil.
/// </summary>
public class LitterBin : MonoBehaviour, IInteractable
{
    public string GetInteractPrompt()
    {
        return "Çöpü at";
    }

    public void Interact(PlayerInteraction player)
    {
        PickupItem held = player.GetHeldItem();

        if (held == null)
            return;

        LitterMarker marker = held.GetComponent<LitterMarker>();

        if (marker == null)
        {
            Debug.Log("Çöp kovası: Elindeki şey çöp değil.");
            return;
        }

        player.SetHeldItem(null);

        // Objeyi orijinal (spawn) konumuna sıfırlayıp kapatıyoruz,
        // böylece AreaLitterZone ileride tekrar aktif edebilir.
        held.ReturnToOriginalPosition();
        held.gameObject.SetActive(false);

        marker.Zone?.NotifyLitterCleaned(held.gameObject);

        Debug.Log("Çöp kovası: Çöp atıldı.");
    }
}