using UnityEngine;

/// <summary>
/// Portafilter, espresso cup, milk pitcher, tamper gibi elle tutulabilen
/// her objeye bu scripti eklersin.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("Ayarlar")]
    [SerializeField] private string itemName = "Item";
    [SerializeField] private float holdSmoothSpeed = 15f;
    [SerializeField] private float surfaceYOffset = 0f;

    [Tooltip("İşaretlenirse bu item SADECE sol elde tutulabilir.")]
    [SerializeField] private bool isLeftHandOnly = false;

    [Header("Elde Tutma Ayarı")]
    [SerializeField] private Vector3 holdPositionOffset = Vector3.zero;
    [SerializeField] private Vector3 holdRotationOffsetEuler = Vector3.zero;

    [Header("Kahve Doldurma")]
    [SerializeField] private GameObject groundCoffeeVisual;
    [SerializeField] private float tampedVisualScaleY = 0.85f;

    [Header("Espresso Doldurma")]
    [SerializeField] private GameObject espressoLiquidVisual;

    private bool isTamped = false;
    private Vector3 groundCoffeeOriginalScale;

    private Rigidbody rb;
    private Collider col;
    private Transform holdPoint;

    private bool isHeld = false;

    private Quaternion uprightRotation;
    private Vector3 originalWorldPosition;

    private bool hasPositionOverride = false;
    private Vector3 overridePosition;
    private Quaternion overrideRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        uprightRotation = transform.rotation;
        originalWorldPosition = transform.position;

        if (groundCoffeeVisual != null)
        {
            groundCoffeeOriginalScale =
                groundCoffeeVisual.transform.localScale;

            groundCoffeeVisual.SetActive(false);
        }

        if (espressoLiquidVisual != null)
            espressoLiquidVisual.SetActive(false);
    }

    // =========================================================
    // KAHVE
    // =========================================================

    public void FillWithGroundCoffee()
    {
        if (groundCoffeeVisual != null)
            groundCoffeeVisual.SetActive(true);
    }

    public void EmptyGroundCoffee()
    {
        if (groundCoffeeVisual != null)
        {
            groundCoffeeVisual.SetActive(false);
            groundCoffeeVisual.transform.localScale =
                groundCoffeeOriginalScale;
        }

        isTamped = false;
    }

    public bool HasGroundCoffee =>
        groundCoffeeVisual != null &&
        groundCoffeeVisual.activeSelf;

    public void TampCoffee()
    {
        isTamped = true;

        if (groundCoffeeVisual != null)
        {
            Vector3 s =
                groundCoffeeVisual.transform.localScale;

            groundCoffeeVisual.transform.localScale =
                new Vector3(
                    s.x,
                    s.y * tampedVisualScaleY,
                    s.z
                );
        }
    }

    public bool IsTamped => isTamped;

    // =========================================================
    // ESPRESSO
    // =========================================================

    public void FillWithEspresso()
    {
        if (espressoLiquidVisual != null)
            espressoLiquidVisual.SetActive(true);
    }

    public bool HasEspresso =>
        espressoLiquidVisual != null &&
        espressoLiquidVisual.activeSelf;

    // =========================================================
    // ETKİLEŞİM
    // =========================================================

    public string GetInteractPrompt()
    {
        return isHeld
            ? $"E - {itemName} bırak"
            : $"E - {itemName} al";
    }

    public void Interact(PlayerInteraction player)
    {
        if (!isHeld)
            PickUp(player);
        else
            Drop(player);
    }

    // =========================================================
    // PICK UP
    // =========================================================

    private void PickUp(PlayerInteraction player)
    {
        holdPoint =
            isLeftHandOnly
            ? player.LeftHoldPoint
            : player.HoldPoint;

        rb.isKinematic = true;
        rb.useGravity = false;

        col.enabled = false;

        isHeld = true;

        if (isLeftHandOnly)
            player.SetLeftHeldItem(this);
        else
            player.SetHeldItem(this);
    }

    // =========================================================
    // DROP
    // =========================================================

    public void Drop(PlayerInteraction player)
    {
        rb.isKinematic = false;
        rb.useGravity = true;

        col.enabled = true;

        isHeld = false;

        holdPoint = null;
        hasPositionOverride = false;

        if (isLeftHandOnly)
            player.SetLeftHeldItem(null);
        else
            player.SetHeldItem(null);
    }

    // =========================================================
    // COUNTER
    // =========================================================

    public void PlaceOnCounter(Vector3 worldPosition)
    {
        transform.position =
            worldPosition + Vector3.up * surfaceYOffset;

        transform.rotation =
            uprightRotation;

        rb.isKinematic = true;
        rb.useGravity = false;

        col.enabled = true;

        isHeld = false;
    }

    public void PlaceAtExact(
        Vector3 worldPosition,
        Quaternion worldRotation)
    {
        transform.position = worldPosition;
        transform.rotation = worldRotation;

        rb.isKinematic = true;
        rb.useGravity = false;

        col.enabled = true;

        isHeld = false;
    }

    // =========================================================
    // FORCE PICKUP
    // =========================================================

    public void ForcePickUp(PlayerInteraction player)
    {
        holdPoint =
            isLeftHandOnly
            ? player.LeftHoldPoint
            : player.HoldPoint;

        rb.isKinematic = true;
        rb.useGravity = false;

        col.enabled = false;

        isHeld = true;

        if (isLeftHandOnly)
            player.SetLeftHeldItem(this);
        else
            player.SetHeldItem(this);
    }

    // =========================================================
    // DOCK
    // =========================================================

    public void DockAt(
        Vector3 worldPosition,
        Vector3 extraRotationEuler)
    {
        transform.position = worldPosition;

        transform.rotation =
            uprightRotation *
            Quaternion.Euler(extraRotationEuler);

        rb.isKinematic = true;
        rb.useGravity = false;

        col.enabled = true;

        isHeld = false;
    }

    // =========================================================
    // EL POZİSYON OVERRIDE
    // =========================================================

    public void OverrideHeldPosition(
        Vector3 worldPos,
        Quaternion worldRot)
    {
        hasPositionOverride = true;

        overridePosition = worldPos;
        overrideRotation = worldRot;
    }

    public void ClearHeldPositionOverride()
    {
        hasPositionOverride = false;
    }

    // =========================================================
    // ORİJİNAL POZİSYONA DÖN
    // =========================================================

    public void ReturnToOriginalPosition()
    {
        transform.position = originalWorldPosition;
        transform.rotation = uprightRotation;

        rb.isKinematic = true;
        rb.useGravity = false;

        col.enabled = true;

        isHeld = false;

        holdPoint = null;
        hasPositionOverride = false;
    }

    // =========================================================
    // BAŞKA OBJENİN ÜZERİNE TAK
    // =========================================================

    public void AttachToObject(
        Transform parent,
        Vector3 worldPosition,
        Quaternion worldRotation)
    {
        transform.position = worldPosition;
        transform.rotation = worldRotation;

        transform.SetParent(parent);

        rb.isKinematic = true;
        rb.useGravity = false;

        col.enabled = false;

        isHeld = false;

        holdPoint = null;
        hasPositionOverride = false;
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!isHeld || holdPoint == null)
            return;

        Vector3 targetPosition;
        Quaternion targetRotation;

        if (hasPositionOverride)
        {
            targetPosition = overridePosition;
            targetRotation = overrideRotation;
        }
        else
        {
            targetPosition =
                holdPoint.TransformPoint(
                    holdPositionOffset
                );

            targetRotation =
                holdPoint.rotation *
                Quaternion.Euler(
                    holdRotationOffsetEuler
                );
        }

        transform.position =
            Vector3.Lerp(
                transform.position,
                targetPosition,
                Time.deltaTime * holdSmoothSpeed
            );

        transform.rotation =
            Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * holdSmoothSpeed
            );
    }

    // =========================================================
    // GETTERS
    // =========================================================

    public bool IsHeld => isHeld;

    public string ItemName => itemName;

    public bool IsLeftHandOnly =>
        isLeftHandOnly;
}