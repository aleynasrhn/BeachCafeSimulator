using UnityEngine;

/// <summary>
/// Espresso makinesinin grup kafası (group head) altına, portafilterin tam oturması
/// gereken noktaya koyulacak küçük/görünmez bir GameObject'e eklenir.
/// GameObject'in Layer'ı "Interactable" olmalı (Machines DEĞİL - o layer sadece obstacle
/// kontrolü için, raycast onu görmüyor).
///
/// CounterSurface'ten farklı olarak: sabit tek nokta, sadece belirli item'ı (portafilter)
/// kabul eder, ve elinde doğru item olmadan buraya bir şey konamaz.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MachineDockPoint : MonoBehaviour, IInteractable
{
    [Tooltip("Buraya sadece bu isimdeki item'lar takılabilsin (PickupItem'daki Item Name ile eşleşmeli)")]
    [SerializeField] private string acceptedItemName = "Portafilter";

    [Tooltip("Item'ın buraya oturduğunda alacağı local pozisyon ofseti (genelde 0,0,0 yeterli)")]
    [SerializeField] private Vector3 dockedLocalPositionOffset = Vector3.zero;

    [Tooltip("Item'ın doğal duruşuna (uprightRotation) EKLENECEK açı - genelde 0,0,0 yeterli, sadece garip bir açıda oturursa buradan ince ayar yap")]
    [SerializeField] private Vector3 dockedExtraRotationEuler = Vector3.zero;

    [Tooltip("İşaretlenirse, item'ın (HasGroundCoffee) içinde kahve olması ZORUNLU olur - boş portafilter takılamaz. Sadece PortafilterDock'ta işaretle.")]
    [SerializeField] private bool requireGroundCoffee = false;

    [Tooltip("İşaretlenirse, item'ın TAMPERLENMİŞ olması ZORUNLU olur. Sadece PortafilterDock'ta işaretle.")]
    [SerializeField] private bool requireTamped = false;

    private bool isOccupied = false;
    private PickupItem dockedItem;

    // Dışarıdan (EspressoMachineButton gibi) okunabilmesi için
    public bool IsOccupied => isOccupied;
    public PickupItem DockedItem => dockedItem;

    public string GetInteractPrompt()
    {
        if (isOccupied) return $"E - {acceptedItemName} çıkar";
        return $"E - {acceptedItemName} tak";
    }

    public void Interact(PlayerInteraction player)
    {
        if (isOccupied)
        {
            // Takılı item'ı geri al
            dockedItem.ForcePickUp(player);
            isOccupied = false;
            dockedItem = null;
            return;
        }

        PickupItem held = player.GetHeldItem();
        if (held == null) return;
        if (held.ItemName != acceptedItemName) return; // yanlış item, kabul etme
        if (requireGroundCoffee && !held.HasGroundCoffee) return; // boş portafilter takılamaz
        if (requireTamped && !held.IsTamped) return; // tamperlenmemiş portafilter takılamaz

        Vector3 worldPos = transform.position + transform.TransformDirection(dockedLocalPositionOffset);
        held.DockAt(worldPos, dockedExtraRotationEuler); // itemin doğal duruşunu korur, dock'un kendi rotasyonunu YOK SAYAR

        dockedItem = held;
        isOccupied = true;
        player.SetHeldItem(null);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isOccupied ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.03f);
    }
}