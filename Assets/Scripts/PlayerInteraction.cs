using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public float interactionDistance = 3f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            PortafilterPickup portafilter =
                hit.collider.GetComponentInParent<PortafilterPickup>();

            if (portafilter != null)
            {
                portafilter.PickUp();
            }
        }
    }
}