using UnityEngine;

/// <summary>
/// Mevcut player/kamera objenize eklenir. Kameranın baktığı yöne raycast atar,
/// IInteractable bulursa E ile (tek basış), IHoldInteractable bulursa E'yi basılı
/// tutarak etkileşime izin verir.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private InteractionUI interactionUI;

    [Header("Ayarlar")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableLayer = ~0;

    private PickupItem currentHeldItem;
    private IInteractable currentTarget;

    private IHoldInteractable currentHoldTarget;
    private float holdTimer = 0f;
    private bool wasShowingPrompt = false;

    public Transform HoldPoint => holdPoint;
    public Vector3 LastHitPoint { get; private set; }

    private void Update()
    {
        HandleRaycast();
        HandleHoldInteraction();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }

        if (Input.GetKeyDown(KeyCode.G) && currentHeldItem != null)
        {
            currentHeldItem.Drop(this);
        }
    }

    private void HandleRaycast()
    {
        currentTarget = null;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            LastHitPoint = hit.point;

            if (hit.collider.TryGetComponent(out IHoldInteractable holdInteractable))
            {
                if (currentHoldTarget != holdInteractable)
                {
                    holdTimer = 0f; // farklı hedef, sayaç sıfırlanır
                }
                currentHoldTarget = holdInteractable;
            }
            else
            {
                currentHoldTarget = null;
            }

            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                currentTarget = interactable;
                return;
            }
        }
        else
        {
            currentHoldTarget = null;
        }
    }

    private void HandleHoldInteraction()
    {
        bool eligible = currentHoldTarget != null && currentHoldTarget.CanStartHold(this);

        // Hedef yok / uygun değilse (elinde yanlış item, zaten dolu vs.) -> her şeyi kapat
        if (!eligible)
        {
            if (holdTimer > 0f) holdTimer = 0f;
            interactionUI?.HideHoldProgress();
            if (wasShowingPrompt) { interactionUI?.HidePrompt(); wasShowingPrompt = false; }
            return;
        }

        // Uygun hedef var ama E'ye henüz basılmıyor -> sadece prompt göster ("E'ye basılı tut")
        if (!Input.GetKey(KeyCode.E))
        {
            if (holdTimer > 0f) holdTimer = 0f;
            interactionUI?.HideHoldProgress();
            interactionUI?.ShowPrompt(currentHoldTarget.GetHoldPrompt());
            wasShowingPrompt = true;
            return;
        }

        // E basılı tutuluyor -> prompt'u kapat, halkayı doldur
        if (wasShowingPrompt) { interactionUI?.HidePrompt(); wasShowingPrompt = false; }

        holdTimer += Time.deltaTime;
        float duration = currentHoldTarget.HoldDuration;
        float progress = holdTimer / duration;
        int secondsRemaining = Mathf.CeilToInt(Mathf.Max(0f, duration - holdTimer));

        interactionUI?.ShowHoldProgress(progress, secondsRemaining);

        if (holdTimer >= duration)
        {
            currentHoldTarget.OnHoldComplete(this);
            holdTimer = 0f;
            interactionUI?.HideHoldProgress();
        }
    }

    private void TryInteract()
    {
        if (currentHeldItem != null && currentTarget is PickupItem targetItem && targetItem != currentHeldItem)
        {
            return;
        }

        currentTarget?.Interact(this);
    }

    public void SetHeldItem(PickupItem item)
    {
        currentHeldItem = item;
    }

    public PickupItem GetHeldItem() => currentHeldItem;
}