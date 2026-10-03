using UnityEngine;

/// <summary>
/// Elle alınamayan yer kirleri (su birikintisi, kafe zemini
/// pisliği vb.). Sadece belirtilen alet (Mop ya da Süpürge) elde
/// tutulurken, E'ye basılı tutularak temizlenir.
///
/// Üstünde AreaLitterZone (tuvalet) ya da CafeFloorLitterZone
/// (kafe zemini) component'i bulunan herhangi bir objenin
/// çocuğu olabilir — hangisi ILitterZone'u uyguluyorsa onu bulur.
/// </summary>
public class SweepableStain : MonoBehaviour, IHoldInteractable
{
    [SerializeField] private float holdDuration = 1.2f;

    [Tooltip("PickupItem'daki Item Name alanıyla BİREBİR aynı olmalı. Örn: 'Mop' ya da 'Süpürge'.")]
    [SerializeField] private string requiredToolItemName = "Mop";

    private ILitterZone zone;

    public float HoldDuration => holdDuration;

    private void Awake()
    {
        zone = GetComponentInParent<ILitterZone>();

        if (zone == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Üstünde AreaLitterZone ya da " +
                "CafeFloorLitterZone bulunamadı!"
            );
        }
    }

    public bool CanStartHold(PlayerInteraction player)
    {
        PickupItem held = player.GetHeldItem();

        return held != null &&
               held.ItemName == requiredToolItemName;
    }

    public string GetHoldPrompt()
    {
        return $"Temizle ({requiredToolItemName} ile E'ye basılı tut)";
    }

    public void OnHoldProgress(PlayerInteraction player, float progress)
    {
    }

    public void OnHoldComplete(PlayerInteraction player)
    {
        gameObject.SetActive(false);

        zone?.NotifyLitterCleaned(gameObject);

        Debug.Log($"{name} temizlendi.");
    }
}