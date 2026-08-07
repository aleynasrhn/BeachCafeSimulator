using UnityEngine;

/// <summary>
/// Portafilter, espresso cup, milk pitcher, tamper gibi elle tutulabilen
/// her objeye bu scripti eklersin. Tek el sistemi - herkes aynı HoldPoint'i kullanır.
///
/// ONEMLI: transform.SetParent() hiç kullanılmıyor (scale drift bug'ı için) - parent hiç
/// değişmiyor, Update() içinde world-space pozisyon/rotasyon elle güncelleniyor.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("Ayarlar")]
    [SerializeField] private string itemName = "Item";
    [SerializeField] private float holdSmoothSpeed = 15f;
    [SerializeField] private float surfaceYOffset = 0f;
    [Tooltip("İşaretlenirse bu item SADECE sol elde tutulabilir (tamper). İşaretsizse sağ elde tutulur (çoğu item).")]
    [SerializeField] private bool isLeftHandOnly = false;

    [Header("Elde Tutma Ayarı (pivot farklıysa buradan düzelt)")]
    [SerializeField] private Vector3 holdPositionOffset = Vector3.zero;
    [SerializeField] private Vector3 holdRotationOffsetEuler = Vector3.zero;

    [Header("Kahve Doldurma (sadece portafilter için, diğer itemlerde boş bırak)")]
    [SerializeField] private GameObject groundCoffeeVisual;
    [SerializeField] private float tampedVisualScaleY = 0.85f;

    [Header("Espresso Doldurma (sadece cup için, diğer itemlerde boş bırak)")]
    [SerializeField] private GameObject espressoLiquidVisual;

    private bool isTamped = false;
    private Vector3 groundCoffeeOriginalScale;

    private Rigidbody rb;
    private Collider col;
    private Transform holdPoint;
    private bool isHeld = false;

    private Quaternion uprightRotation;
    private Vector3 originalWorldPosition; // tamper "yerine geri dönerken" kullanılır

    // TamperableCoffee gibi scriptlerin, basılı tutma sırasında elin pozisyonunu
    // geçici olarak (animasyon için) elle kontrol etmesi için
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
            groundCoffeeOriginalScale = groundCoffeeVisual.transform.localScale;
            groundCoffeeVisual.SetActive(false);
        }

        if (espressoLiquidVisual != null)
            espressoLiquidVisual.SetActive(false);
    }

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
            groundCoffeeVisual.transform.localScale = groundCoffeeOriginalScale;
        }
        isTamped = false;
    }

    public bool HasGroundCoffee => groundCoffeeVisual != null && groundCoffeeVisual.activeSelf;

    public void TampCoffee()
    {
        isTamped = true;
        if (groundCoffeeVisual != null)
        {
            Vector3 s = groundCoffeeVisual.transform.localScale;
            groundCoffeeVisual.transform.localScale = new Vector3(s.x, s.y * tampedVisualScaleY, s.z);
        }
    }

    public bool IsTamped => isTamped;

    public void FillWithEspresso()
    {
        if (espressoLiquidVisual != null)
            espressoLiquidVisual.SetActive(true);
    }

    public bool HasEspresso => espressoLiquidVisual != null && espressoLiquidVisual.activeSelf;

    public string GetInteractPrompt()
    {
        return isHeld ? $"E - {itemName} bırak" : $"E - {itemName} al";
    }

    public void Interact(PlayerInteraction player)
    {
        if (!isHeld)
            PickUp(player);
        else
            Drop(player);
    }

    private void PickUp(PlayerInteraction player)
    {
        holdPoint = isLeftHandOnly ? player.LeftHoldPoint : player.HoldPoint;

        rb.isKinematic = true;
        rb.useGravity = false;
        col.enabled = false;

        isHeld = true;

        if (isLeftHandOnly) player.SetLeftHeldItem(this);
        else player.SetHeldItem(this);
    }

    public void Drop(PlayerInteraction player)
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        col.enabled = true;

        isHeld = false;
        holdPoint = null;
        hasPositionOverride = false;

        if (isLeftHandOnly) player.SetLeftHeldItem(null);
        else player.SetHeldItem(null);
    }

    public void PlaceOnCounter(Vector3 worldPosition)
    {
        transform.position = worldPosition + Vector3.up * surfaceYOffset;
        transform.rotation = uprightRotation;

        rb.isKinematic = true;
        rb.useGravity = false;
        col.enabled = true;

        isHeld = false;
    }

    public void PlaceAtExact(Vector3 worldPosition, Quaternion worldRotation)
    {
        transform.position = worldPosition;
        transform.rotation = worldRotation;

        rb.isKinematic = true;
        rb.useGravity = false;
        col.enabled = true;

        isHeld = false;
    }

    public void ForcePickUp(PlayerInteraction player)
    {
        holdPoint = isLeftHandOnly ? player.LeftHoldPoint : player.HoldPoint;

        rb.isKinematic = true;
        rb.useGravity = false;
        col.enabled = false;

        isHeld = true;

        if (isLeftHandOnly) player.SetLeftHeldItem(this);
        else player.SetHeldItem(this);
    }

    public void DockAt(Vector3 worldPosition, Vector3 extraRotationEuler)
    {
        transform.position = worldPosition;
        transform.rotation = uprightRotation * Quaternion.Euler(extraRotationEuler);

        rb.isKinematic = true;
        rb.useGravity = false;
        col.enabled = true;

        isHeld = false;
    }

    /// <summary>TamperableCoffee gibi scriptler, animasyon sırasında elin pozisyonunu
    /// geçici olarak ezmek için çağırır.</summary>
    public void OverrideHeldPosition(Vector3 worldPos, Quaternion worldRot)
    {
        hasPositionOverride = true;
        overridePosition = worldPos;
        overrideRotation = worldRot;
    }

    public void ClearHeldPositionOverride()
    {
        hasPositionOverride = false;
    }

    /// <summary>Item'ı oyunun başında durduğu ORİJİNAL pozisyona/rotasyona geri koyar
    /// (tamper işini bitirince otomatik "yerine dönmesi" için).</summary>
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

    private void Update()
    {
        if (!isHeld || holdPoint == null) return;

        Vector3 targetPosition;
        Quaternion targetRotation;

        if (hasPositionOverride)
        {
            targetPosition = overridePosition;
            targetRotation = overrideRotation;
        }
        else
        {
            targetPosition = holdPoint.TransformPoint(holdPositionOffset);
            targetRotation = holdPoint.rotation * Quaternion.Euler(holdRotationOffsetEuler);
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * holdSmoothSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * holdSmoothSpeed);
    }

    public bool IsHeld => isHeld;
    public string ItemName => itemName;
    public bool IsLeftHandOnly => isLeftHandOnly;
}