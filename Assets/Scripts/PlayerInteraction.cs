using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint;

    [Header("Settings")]
    [SerializeField] private float interactDistance = 3f;

    private PickupItem heldItem;

    private void Update()
    {
        CheckInteraction();
    }

    private void CheckInteraction()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (Input.GetKeyDown(KeyCode.E) && heldItem == null)
            {
                PickupItem item = hit.collider.GetComponentInParent<PickupItem>();

                if (item != null)
                {
                    heldItem = item;
                    heldItem.PickUp(holdPoint);
                }
            }

            if (Input.GetKeyDown(KeyCode.F) && heldItem != null)
            {
                PlaceableSurface surface = hit.collider.GetComponentInParent<PlaceableSurface>();

                if (surface != null)
                {
                    Vector3 placePosition = hit.point + Vector3.up * heldItem.placeHeight;

                    heldItem.Place(placePosition);
                    heldItem = null;
                }
            }
        }
    }
}