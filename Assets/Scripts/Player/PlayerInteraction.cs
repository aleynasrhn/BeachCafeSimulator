using UnityEngine;

/// <summary>
/// E: Bakılan nesneyle etkileşim.
/// PC'ye bakılıyorsa ComputerInteraction sistemini çalıştırır.
/// Kapak eldeyken bardağa bakılırsa kapağı takar.
/// Takılı kapağa bakılıp E'ye basılırsa kapağı bardaktan ayırıp ele alır.
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

    private CupLidReceiver currentCupLidReceiver;

    private IHoldInteractable currentHoldTarget;

    private float holdTimer = 0f;
    private bool wasShowingPrompt = false;

    public Transform HoldPoint => holdPoint;
    public Transform LeftHoldPoint => leftHoldPoint;

    public Vector3 LastHitPoint { get; private set; }


    // =========================================================
    // INTERACTABLE KONTROLÜ
    // =========================================================

    public bool IsLookingAtInteractable =>
        (currentTarget != null &&
         !(currentTarget is CounterSurface))
        || currentHoldTarget != null
        || currentComputer != null
        || currentCupLidReceiver != null;


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        HandleRaycast();
        HandleHoldInteraction();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }


    // =========================================================
    // RAYCAST
    // =========================================================

    private void HandleRaycast()
    {
        currentTarget = null;
        currentComputer = null;
        currentCupLidReceiver = null;


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


            // ==========================================
            // PC
            // ==========================================

            currentComputer =
                hit.collider.GetComponentInParent<
                    ComputerInteraction
                >();


            // ==========================================
            // CUP / LID RECEIVER
            // ==========================================

            currentCupLidReceiver =
                hit.collider.GetComponentInParent<
                    CupLidReceiver
                >();


            // ==========================================
            // HOLD SYSTEM
            // ==========================================

            if (hit.collider.TryGetComponent(
                out IHoldInteractable holdInteractable))
            {
                if (currentHoldTarget != holdInteractable)
                {
                    holdTimer = 0f;
                    ClearAnyHoldOverride();
                }

                currentHoldTarget =
                    holdInteractable;
            }
            else
            {
                if (currentHoldTarget != null)
                {
                    ClearAnyHoldOverride();
                }

                currentHoldTarget = null;
            }


            // ==========================================
            // NORMAL INTERACTION
            // ==========================================

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
            {
                ClearAnyHoldOverride();
            }

            currentHoldTarget = null;
        }
    }


    // =========================================================
    // HOLD INTERACTION
    // =========================================================

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


        float duration =
            currentHoldTarget.HoldDuration;


        float progress =
            holdTimer / duration;


        int secondsRemaining =
            Mathf.CeilToInt(
                Mathf.Max(
                    0f,
                    duration - holdTimer
                )
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


    // =========================================================
    // HOLD OVERRIDE TEMİZLE
    // =========================================================

    private void ClearAnyHoldOverride()
    {
        currentHeldItem?.ClearHeldPositionOverride();

        currentLeftHeldItem?.ClearHeldPositionOverride();
    }


    // =========================================================
    // ANA ETKİLEŞİM
    // =========================================================

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
        // TAKILI KAPAĞI AYIR
        // ==========================================

        if (currentTarget is PickupItem targetPickup)
        {
            // Bakılan şey PickupItem ise ve başka objeye
            // takılı bir kapaksa
            if (targetPickup.IsAttachedToObject)
            {
                LidItem lidItem =
                    targetPickup.GetComponent<LidItem>();


                if (lidItem != null)
                {
                    // Sağ el doluysa başka item alamıyoruz
                    if (currentHeldItem != null)
                    {
                        return;
                    }


                    // Kapağı bardaktan ayır
                    targetPickup.DetachFromObject();


                    // Ardından doğrudan ele al
                    targetPickup.ForcePickUp(this);


                    return;
                }
            }
        }


        // ==========================================
        // KAPAK TAKMA
        // ==========================================

        if (currentCupLidReceiver != null)
        {
            // --------------------------------------
            // SAĞ ELDE KAPAK
            // --------------------------------------

            if (currentHeldItem != null)
            {
                LidItem lid =
                    currentHeldItem.GetComponent<LidItem>();


                if (lid != null)
                {
                    bool attached =
                        currentCupLidReceiver.TryAttachLid(
                            currentHeldItem
                        );


                    if (attached)
                    {
                        // Kapak artık elde değil
                        currentHeldItem = null;
                    }


                    return;
                }
            }


            // --------------------------------------
            // SOL ELDE KAPAK
            // --------------------------------------

            if (currentLeftHeldItem != null)
            {
                LidItem lid =
                    currentLeftHeldItem.GetComponent<LidItem>();


                if (lid != null)
                {
                    bool attached =
                        currentCupLidReceiver.TryAttachLid(
                            currentLeftHeldItem
                        );


                    if (attached)
                    {
                        currentLeftHeldItem = null;
                    }


                    return;
                }
            }
        }


        // ==========================================
        // NORMAL PICKUP KONTROLÜ
        // ==========================================

        if (currentTarget is PickupItem targetItem)
        {
            // Sol el
            if (targetItem.IsLeftHandOnly)
            {
                if (currentLeftHeldItem != null &&
                    targetItem != currentLeftHeldItem)
                {
                    return;
                }
            }
            // Sağ el
            else
            {
                if (currentHeldItem != null &&
                    targetItem != currentHeldItem)
                {
                    return;
                }
            }
        }


        // ==========================================
        // NORMAL ETKİLEŞİM
        // ==========================================

        currentTarget?.Interact(this);
    }


    // =========================================================
    // SAĞ EL
    // =========================================================

    public void SetHeldItem(PickupItem item)
    {
        currentHeldItem = item;
    }


    public PickupItem GetHeldItem()
    {
        return currentHeldItem;
    }


    // =========================================================
    // SOL EL
    // =========================================================

    public void SetLeftHeldItem(PickupItem item)
    {
        currentLeftHeldItem = item;
    }


    public PickupItem GetLeftHeldItem()
    {
        return currentLeftHeldItem;
    }
}