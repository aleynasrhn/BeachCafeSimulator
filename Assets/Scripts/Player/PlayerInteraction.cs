using UnityEngine;

/// <summary>
/// E: Bakılan nesneyle etkileşim.
/// PC'ye bakılıyorsa ComputerInteraction sistemini çalıştırır.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform leftHoldPoint;
    [SerializeField] private InteractionUI interactionUI;

    [Header("Ayarlar")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableLayer = ~0;

    private PickupItem currentHeldItem;
    private PickupItem currentLeftHeldItem;

    private IInteractable currentTarget;
    private ComputerInteraction currentComputer;

    private IHoldInteractable currentHoldTarget;
    private float holdTimer = 0f;
    private bool wasShowingPrompt = false;

    public Transform HoldPoint => holdPoint;
    public Transform LeftHoldPoint => leftHoldPoint;

    public Vector3 LastHitPoint { get; private set; }

    public bool IsLookingAtInteractable =>
        (currentTarget != null && !(currentTarget is CounterSurface))
        || currentHoldTarget != null
        || currentComputer != null;


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
        currentComputer = null;

        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactRange,
            interactableLayer))
        {
            LastHitPoint = hit.point;

            // PC kontrolü
            currentComputer =
                hit.collider.GetComponentInParent<ComputerInteraction>();

            // Hold sistemi
            if (hit.collider.TryGetComponent(
                out IHoldInteractable holdInteractable))
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
                if (currentHoldTarget != null)
                    ClearAnyHoldOverride();

                currentHoldTarget = null;
            }

            // Normal etkileşim
            if (hit.collider.TryGetComponent(
                out IInteractable interactable))
            {
                currentTarget = interactable;
                return;
            }
        }
        else
        {
            if (currentHoldTarget != null)
                ClearAnyHoldOverride();

            currentHoldTarget = null;
        }
    }


    private void HandleHoldInteraction()
    {
        bool eligible =
            currentHoldTarget != null &&
            currentHoldTarget.CanStartHold(this);

        if (!eligible)
        {
            if (holdTimer > 0f)
            {
                holdTimer = 0f;
                ClearAnyHoldOverride();
            }

            interactionUI?.HideHoldProgress();

            if (wasShowingPrompt)
            {
                interactionUI?.HidePrompt();
                wasShowingPrompt = false;
            }

            return;
        }

        if (!Input.GetKey(KeyCode.E))
        {
            if (holdTimer > 0f)
            {
                holdTimer = 0f;
                ClearAnyHoldOverride();
            }

            interactionUI?.HideHoldProgress();

            interactionUI?.ShowPrompt(
                currentHoldTarget.GetHoldPrompt()
            );

            wasShowingPrompt = true;

            return;
        }

        if (wasShowingPrompt)
        {
            interactionUI?.HidePrompt();
            wasShowingPrompt = false;
        }

        holdTimer += Time.deltaTime;

        float duration = currentHoldTarget.HoldDuration;

        float progress = holdTimer / duration;

        int secondsRemaining =
            Mathf.CeilToInt(
                Mathf.Max(0f, duration - holdTimer)
            );

        interactionUI?.ShowHoldProgress(
            progress,
            secondsRemaining
        );

        currentHoldTarget.OnHoldProgress(
            this,
            Mathf.Clamp01(progress)
        );

        if (holdTimer >= duration)
        {
            currentHoldTarget.OnHoldComplete(this);

            holdTimer = 0f;

            interactionUI?.HideHoldProgress();
        }
    }


    private void ClearAnyHoldOverride()
    {
        currentHeldItem?.ClearHeldPositionOverride();
        currentLeftHeldItem?.ClearHeldPositionOverride();
    }


    private void TryInteract()
    {
        // ==========================================
        // PC
        // ==========================================

        if (currentComputer != null)
        {
            currentComputer.EnterComputer();
            return;
        }


        // ==========================================
        // NORMAL ETKİLEŞİMLER
        // ==========================================

        if (currentTarget is PickupItem targetItem)
        {
            if (targetItem.IsLeftHandOnly)
            {
                if (currentLeftHeldItem != null &&
                    targetItem != currentLeftHeldItem)
                {
                    return;
                }
            }
            else
            {
                if (currentHeldItem != null &&
                    targetItem != currentHeldItem)
                {
                    return;
                }
            }
        }

        currentTarget?.Interact(this);
    }


    public void SetHeldItem(PickupItem item)
    {
        currentHeldItem = item;
    }


    public PickupItem GetHeldItem()
    {
        return currentHeldItem;
    }


    public void SetLeftHeldItem(PickupItem item)
    {
        currentLeftHeldItem = item;
    }


    public PickupItem GetLeftHeldItem()
    {
        return currentLeftHeldItem;
    }
}