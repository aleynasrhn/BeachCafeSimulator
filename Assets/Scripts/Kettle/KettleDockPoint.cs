using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KettleDockPoint : MonoBehaviour, IInteractable
{
    [Header("Kabul Edilen Item")]
    [SerializeField] private string acceptedItemName = "Kettle";

    [Header("Yerleşim")]
    [SerializeField]
    private Vector3 positionOffset =
        Vector3.zero;

    [SerializeField]
    private Vector3 rotationOffsetEuler =
        Vector3.zero;

    private bool isOccupied = false;
    private PickupItem dockedItem;


    public bool IsOccupied =>
        isOccupied;

    public PickupItem DockedItem =>
        dockedItem;


    // =========================================================
    // PROMPT
    // =========================================================

    public string GetInteractPrompt()
    {
        return "";
    }


    // =========================================================
    // ETKİLEŞİM
    // =========================================================

    public void Interact(PlayerInteraction player)
    {
        if (player == null)
            return;


        // =====================================================
        // DOCK DOLUYSA → KETTLE'I AL
        // =====================================================

        if (isOccupied)
        {
            if (dockedItem != null)
            {
                // Kettle ısınıyorsa alınamaz
                KettleHeatController kettleHeat =
                    dockedItem.GetComponent<KettleHeatController>();

                if (kettleHeat != null &&
                    kettleHeat.IsHeating)
                {
                    Debug.Log(
                        "Kettle ısınıyor, şu anda alınamaz."
                    );

                    return;
                }


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
        // HEDEF POZİSYON
        // =====================================================

        Vector3 targetPosition =
            transform.position +
            transform.TransformDirection(
                positionOffset
            );


        // =====================================================
        // HEDEF ROTASYON
        // =====================================================

        Quaternion targetRotation =
            transform.rotation *
            Quaternion.Euler(
                rotationOffsetEuler
            );


        // =====================================================
        // KETTLE'I YERLEŞTİR
        // =====================================================

        held.PlaceAtExact(
            targetPosition,
            targetRotation
        );


        dockedItem = held;

        isOccupied = true;


        player.SetHeldItem(null);


        Debug.Log(
            "Kettle dock noktasına yerleştirildi."
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