using UnityEngine;

/// <summary>
/// Mevcut player/kamera objenize eklenir. Kameranın baktığı yöne raycast atar,
/// IInteractable bulursa E ile etkileşime izin verir.
/// HoldPoint objesini Inspector'dan sürükleyip atamayı unutma.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Camera playerCamera;   // Main Camera'yı sürükle
    [SerializeField] private Transform holdPoint;    // Hierarchy'deki HoldPoint'i sürükle

    [Header("Ayarlar")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableLayer = ~0; // varsayılan: her şey

    private PickupItem currentHeldItem;
    private IInteractable currentTarget;

    public Transform HoldPoint => holdPoint;
    public Vector3 LastHitPoint { get; private set; } // raycast'in çarptığı tam dünya koordinatı

    private void Update()
    {
        HandleRaycast();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }

        // Herhangi bir placement point'e bakmadan da elindekini bırakabilmek için (opsiyonel)
        if (Input.GetKeyDown(KeyCode.G) && currentHeldItem != null)
        {
            currentHeldItem.Drop(this);
        }
    }

    private void HandleRaycast()
    {
        currentTarget = null;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); // ekranın tam ortası (crosshair)
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            LastHitPoint = hit.point;

            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                currentTarget = interactable;
                // Burada UI prompt'unu güncelleyebilirsin:
                // UIManager.Instance.ShowPrompt(interactable.GetInteractPrompt());
                return;
            }
        }

        // Hedef yoksa prompt'u gizle:
        // UIManager.Instance.HidePrompt();
    }

    private void TryInteract()
    {
        // Elimde bir şey varsa ve hedef başka bir pickup item ise -> önce elimdekini bırakmadan yenisini alma
        if (currentHeldItem != null && currentTarget is PickupItem targetItem && targetItem != currentHeldItem)
        {
            return; // istersen burada "önce elindekini bırak" mesajı gösterebilirsin
        }

        currentTarget?.Interact(this);
    }

    public void SetHeldItem(PickupItem item)
    {
        currentHeldItem = item;
    }

    public PickupItem GetHeldItem() => currentHeldItem;
}