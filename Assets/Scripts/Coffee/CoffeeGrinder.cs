using UnityEngine;

public class CoffeeGrinder : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform grindPoint;
    [SerializeField] private float grindDistance = 0.35f;

    public void Interact()
    {
        if (PickupItem.HeldItem == null)
            return;

        if (PickupItem.HeldItem.ItemType != ItemType.Portafilter)
            return;

        float distance = Vector3.Distance(
            PickupItem.HeldItem.transform.position,
            grindPoint.position
        );

        if (distance > grindDistance)
            return;

        PortafilterPickup portafilter =
            PickupItem.HeldItem.GetComponent<PortafilterPickup>();

        if (portafilter == null)
            return;

        portafilter.FillWithCoffee();

        Debug.Log("Kahve öğütüldü!");
    }
}