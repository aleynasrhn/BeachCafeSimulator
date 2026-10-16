using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorTriggerZone : MonoBehaviour
{
    [SerializeField] private DoorController doorController;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsRelevant(other))
            return;

        if (doorController != null)
        {
            doorController.NotifyCustomerEntered();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsRelevant(other))
            return;

        if (doorController != null)
        {
            doorController.NotifyCustomerExited();
        }
    }

    // =========================================================
    // NPC YA DA PLAYER MI?
    // =========================================================

    private bool IsRelevant(Collider other)
    {
        if (other.GetComponentInParent<NPCController>() != null)
            return true;

        if (other.GetComponentInParent<PlayerMovement>() != null)
            return true;

        return false;
    }
}