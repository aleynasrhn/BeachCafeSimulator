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
        Debug.Log($"[DoorTrigger] Bir şey girdi: {other.gameObject.name}");

        NPCController npc =
            other.GetComponentInParent<NPCController>();

        if (npc == null)
            return;

        if (doorController != null)
        {
            doorController.NotifyCustomerEntered();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        NPCController npc =
            other.GetComponentInParent<NPCController>();

        if (npc == null)
            return;

        if (doorController != null)
        {
            doorController.NotifyCustomerExited();
        }
    }
}