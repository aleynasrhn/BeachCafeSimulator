using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("Hold Settings")]
    [SerializeField] private Vector3 holdPosition;
    [SerializeField] private Vector3 holdRotation;
    [SerializeField] private ItemType itemType;

    private Transform holdPoint;

    public bool IsHeld { get; private set; }
    public static PickupItem HeldItem;
    public ItemType ItemType => itemType;
    private void Start()
    {
        GameObject point = GameObject.Find("HoldPoint");

        if (point != null)
            holdPoint = point.transform;
    }

    public void PickUp()
    {
        if (IsHeld)
            return;

        if (HeldItem != null)
            return;

        IsHeld = true;
        HeldItem = this;

        transform.SetParent(holdPoint);

        transform.localPosition = holdPosition;
        transform.localRotation = Quaternion.Euler(holdRotation);
    }

    public void Drop()
    {
        IsHeld = false;
        HeldItem = null;

        transform.SetParent(null);
    }

    public void Interact()
    {
        PickUp();
    }
}