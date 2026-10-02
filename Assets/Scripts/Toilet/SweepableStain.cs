using UnityEngine;

/// <summary>
/// Elle alınamayan yer kirleri (su birikintisi vb.). Sadece
/// belirtilen alet (Mop ya da Süpürge) elde tutulurken, E'ye
/// basılı tutularak temizlenir. Her obje kendi aletini seçer.
/// </summary>
public class SweepableStain : MonoBehaviour, IHoldInteractable
{
    [SerializeField] private float holdDuration = 1.2f;

    [Tooltip("PickupItem'daki Item Name alanıyla BİREBİR aynı olmalı. Örn: 'Mop' ya da 'Süpürge'.")]
    [SerializeField] private string requiredToolItemName = "Mop";

    private AreaLitterZone zone;

    public float HoldDuration => holdDuration;

    private void Awake()
    {
        zone = GetComponentInParent<AreaLitterZone>();
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