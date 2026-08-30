using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KettleDockPoint : MonoBehaviour, IInteractable
{
    [Header("Kabul Edilen Item")]
    [SerializeField] private string acceptedItemName = "Kettle";

    [Header("Yerleşim")]
    [SerializeField] private Vector3 positionOffset = Vector3.zero;
    [SerializeField] private Vector3 rotationOffsetEuler = Vector3.zero;

    private bool isOccupied = false;
    private PickupItem dockedItem;

    public bool IsOccupied => isOccupied;
    public PickupItem DockedItem => dockedItem;


    // =========================================================
    // PROMPT
    // =========================================================

    public string GetInteractPrompt()
    {
        if (isOccupied)
            return $"E - {acceptedItemName} çıkar";

        return $"E - {acceptedItemName} koy";
    }


    // =========================================================
    // ETKİLEŞİM
    // =========================================================

    public void Interact(PlayerInteraction player)
    {
        // =====================================================
        // DOCK DOLUYSA → KETTLE'I AL
        // =====================================================

        if (isOccupied)
        {
            if (dockedItem != null)
            {
                dockedItem.ForcePickUp(player);
            }

            isOccupied = false;
            dockedItem = null;

            return;
        }


        // =====================================================
        // ELDEKİ ITEM
        // =====================================================

        PickupItem held =
            player.GetHeldItem();

        if (held == null)
            return;

        if (held.ItemName != acceptedItemName)
            return;


        // =====================================================
        // POZİSYON
        // =====================================================

        Vector3 targetPosition =
            transform.position +
            transform.TransformDirection(
                positionOffset
            );


        // =====================================================
        // ROTASYON
        // =====================================================

        Quaternion targetRotation =
            transform.rotation *
            Quaternion.Euler(
                rotationOffsetEuler
            );


        // =====================================================
        // TAM OLARAK YERLEŞTİR
        // =====================================================

        held.PlaceAtExact(
            targetPosition,
            targetRotation
        );


        // =====================================================
        // KAYDET
        // =====================================================

        dockedItem = held;
        isOccupied = true;

        player.SetHeldItem(null);


        Debug.Log(
            "Kettle dock noktasına düzgün şekilde yerleştirildi."
        );
    }


    // =========================================================
    // GİZMO
    // =========================================================

    private void OnDrawGizmos()
    {
        Gizmos.color =
            isOccupied
            ? Color.red
            : Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            0.06f
        );
    }
}