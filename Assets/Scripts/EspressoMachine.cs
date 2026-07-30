using UnityEngine;

public class EspressoMachine : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform cupSlot;

    public bool HasCup { get; private set; }

    public void PlaceCup(PickupItem cupItem)
    {
        if (HasCup)
            return;

        HasCup = true;

        cupItem.Drop();

        cupItem.transform.SetParent(cupSlot);
        cupItem.transform.localPosition = Vector3.zero;
        cupItem.transform.localRotation = Quaternion.identity;
    }

    public void Interact()
    {
        if (HasCup)
            return;

        if (PickupItem.HeldItem == null)
            return;

        if (PickupItem.HeldItem.ItemType != ItemType.Cup)
            return;

        PlaceCup(PickupItem.HeldItem);
    }
}