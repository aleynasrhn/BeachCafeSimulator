using UnityEngine;

/// <summary>
/// Tek elle çalışan basit sistem. E: bakılan şeyle etkileşim (al/bırak/tak/basılı tut - hepsi
/// hedefin kendi Interact/OnHoldComplete mantığına göre). Hiçbir yere bakmıyorken elindeki
/// itemi bırakmak istersen F'ye bas.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform leftHoldPoint; // SADECE tamper gibi isLeftHandOnly itemlar için
    [SerializeField] private InteractionUI interactionUI;

    [Header("Ayarlar")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableLayer = ~0;

    private PickupItem currentHeldItem;
    private PickupItem currentLeftHeldItem;
    private IInteractable currentTarget;

    private IHoldInteractable currentHoldTarget;
    private float holdTimer = 0f;
    private bool wasShowingPrompt = false;

    public Transform HoldPoint => holdPoint;
    public Transform LeftHoldPoint => leftHoldPoint;
    public Vector3 LastHitPoint { get; private set; }
    public bool IsLookingAtInteractable => (currentTarget != null && !(currentTarget is CounterSurface)) || currentHoldTarget != null;

    private void Update()
    {
        HandleRaycast();
        HandleHoldInteraction();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
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
                    holdTimer = 0f;
                    ClearAnyHoldOverride();
                }
                currentHoldTarget = holdInteractable;
            }
            else
            {
                if (currentHoldTarget != null) ClearAnyHoldOverride();
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
            if (currentHoldTarget != null) ClearAnyHoldOverride();
            currentHoldTarget = null;
        }
    }

    private void HandleHoldInteraction()
    {
        bool eligible = currentHoldTarget != null && currentHoldTarget.CanStartHold(this);

        if (!eligible)
        {
            if (holdTimer > 0f) { holdTimer = 0f; ClearAnyHoldOverride(); }
            interactionUI?.HideHoldProgress();
            if (wasShowingPrompt) { interactionUI?.HidePrompt(); wasShowingPrompt = false; }
            return;
        }

        if (!Input.GetKey(KeyCode.E))
        {
            if (holdTimer > 0f) { holdTimer = 0f; ClearAnyHoldOverride(); }
            interactionUI?.HideHoldProgress();
            interactionUI?.ShowPrompt(currentHoldTarget.GetHoldPrompt());
            wasShowingPrompt = true;
            return;
        }

        if (wasShowingPrompt) { interactionUI?.HidePrompt(); wasShowingPrompt = false; }

        holdTimer += Time.deltaTime;
        float duration = currentHoldTarget.HoldDuration;
        float progress = holdTimer / duration;
        int secondsRemaining = Mathf.CeilToInt(Mathf.Max(0f, duration - holdTimer));

        interactionUI?.ShowHoldProgress(progress, secondsRemaining);
        currentHoldTarget.OnHoldProgress(this, Mathf.Clamp01(progress));

        if (holdTimer >= duration)
        {
            currentHoldTarget.OnHoldComplete(this);
            holdTimer = 0f;
            interactionUI?.HideHoldProgress();
        }
    }

    // Hold iptal olduğunda (hedef değişti, E bırakıldı vs.) elde animasyon override kalmışsa temizle.
    // NOT: TamperableCoffee kendi "settle" coroutine'i sırasında override'ı KENDİSİ yönetir,
    // bu yüzden hold tamamlandıktan sonra (holdTimer sıfırlandıktan sonra) burası tekrar
    // override'ı temizlemeye ÇALIŞMAZ çünkü o noktada zaten currentHoldTarget/eligible false olur
    // ve coroutine kendi ClearHeldPositionOverride'ını çağırana kadar item'ın override'ı korunur.
    private void ClearAnyHoldOverride()
    {
        currentHeldItem?.ClearHeldPositionOverride();
        currentLeftHeldItem?.ClearHeldPositionOverride();
    }

    private void TryInteract()
    {
        if (currentTarget is PickupItem targetItem)
        {
            if (targetItem.IsLeftHandOnly)
            {
                if (currentLeftHeldItem != null && targetItem != currentLeftHeldItem) return; // sol el dolu
            }
            else
            {
                if (currentHeldItem != null && targetItem != currentHeldItem) return; // sağ el dolu
            }
        }

        currentTarget?.Interact(this);
    }

    public void SetHeldItem(PickupItem item) => currentHeldItem = item;
    public PickupItem GetHeldItem() => currentHeldItem;

    public void SetLeftHeldItem(PickupItem item) => currentLeftHeldItem = item;
    public PickupItem GetLeftHeldItem() => currentLeftHeldItem;
}