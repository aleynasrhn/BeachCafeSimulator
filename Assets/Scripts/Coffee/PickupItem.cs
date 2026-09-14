using System;
using UnityEngine;

/// <summary>
/// Portafilter, espresso cup, milk pitcher, tamper, kettle gibi
/// elle tutulabilen objelere eklenir.
///
/// Tek el sistemi kullanır.
/// Kapak gibi başka bir objeye takılan item'lar da bu scripti kullanabilir.
///
/// Ayrıca:
/// - Kahve doldurma
/// - Tamp durumu
/// - Kullanılmış kahve durumu
/// - Espresso durumu
/// - Geçici etkileşim kilidi
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


    // =========================================================
    // KAHVE DURUMLARI
    // =========================================================

    private bool isTamped = false;

    // Portafilter içinde kullanılmış kahve var mı?
    private bool hasUsedCoffee = false;

    private Vector3 groundCoffeeOriginalScale;


    // =========================================================
    // PHYSICS
    // =========================================================

    private Rigidbody rb;
    private Collider col;


    // =========================================================
    // COLLIDER TRIGGER KONTROLÜ
    // =========================================================

    public void SetColliderTrigger(bool trigger)
    {
        if (col != null)
        {
            col.isTrigger = trigger;
        }
    }


    // =========================================================
    // ELDE TUTMA
    // =========================================================

    private Transform holdPoint;
    private bool isHeld = false;


    // =========================================================
    // OYUNCU BU ITEM'I ELİNE ALDIĞINDA TETİKLENİR
    // =========================================================
    //
    // Masaya bırakılmış bir bardağın oyuncu tarafından geri
    // alındığını (DrinkPlacePoint gibi dış sistemlerin haberdar
    // olması için) bildirmek amacıyla eklendi.
    //
    // =========================================================

    public event Action<PickupItem> OnPickedUp;


    // =========================================================
    // POZİSYON
    // =========================================================

    private Quaternion uprightRotation;
    private Vector3 originalWorldPosition;

    private bool hasPositionOverride = false;
    private Vector3 overridePosition;
    private Quaternion overrideRotation;


    // =========================================================
    // ETKİLEŞİM KİLİDİ
    // =========================================================

    // Tamper, başka bir işlem vb. sırasında item alınamasın.
    private bool isInteractionLocked = false;


    // =========================================================
    // BAŞKA OBJeye TAKILMA SİSTEMİ
    // =========================================================

    private Transform attachedParent;
    private bool isAttachedToObject = false;

    public bool IsAttachedToObject =>
        isAttachedToObject;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();


        uprightRotation =
            transform.rotation;

        originalWorldPosition =
            transform.position;


        // -----------------------------------------------------
        // GROUND COFFEE
        // -----------------------------------------------------

        if (groundCoffeeVisual != null)
        {
            groundCoffeeOriginalScale =
                groundCoffeeVisual
                    .transform
                    .localScale;

            groundCoffeeVisual.SetActive(false);
        }


        // -----------------------------------------------------
        // ESPRESSO
        // -----------------------------------------------------

        if (espressoLiquidVisual != null)
        {
            espressoLiquidVisual.SetActive(false);
        }
    }


    // =========================================================
    // KAHVE
    // =========================================================

    public void FillWithGroundCoffee()
    {
        hasUsedCoffee = false;
        isTamped = false;


        if (groundCoffeeVisual != null)
        {
            groundCoffeeVisual.SetActive(true);

            groundCoffeeVisual.transform.localScale =
                groundCoffeeOriginalScale;
        }
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
        hasUsedCoffee = false;
    }


    public bool HasGroundCoffee =>
        groundCoffeeVisual != null &&
        groundCoffeeVisual.activeSelf;


    public bool HasUsedCoffee =>
        hasUsedCoffee;


    // =========================================================
    // TAMP
    // =========================================================

    public void TampCoffee()
    {
        isTamped = true;


        if (groundCoffeeVisual != null)
        {
            Vector3 s =
                groundCoffeeVisual
                    .transform
                    .localScale;


            groundCoffeeVisual.transform.localScale =
                new Vector3(
                    s.x,
                    s.y * tampedVisualScaleY,
                    s.z
                );
        }
    }


    public bool IsTamped =>
        isTamped;


    // =========================================================
    // USED COFFEE
    // =========================================================

    public void MarkCoffeeAsUsed()
    {
        hasUsedCoffee = true;

        isTamped = false;
    }


    // =========================================================
    // ESPRESSO
    // =========================================================

    public void FillWithEspresso()
    {
        if (espressoLiquidVisual != null)
        {
            espressoLiquidVisual.SetActive(true);
        }
    }


    public bool HasEspresso =>
        espressoLiquidVisual != null &&
        espressoLiquidVisual.activeSelf;


    public void EmptyEspresso()
    {
        if (espressoLiquidVisual != null)
        {
            espressoLiquidVisual.SetActive(false);
        }
    }


    // =========================================================
    // ETKİLEŞİM
    // =========================================================

    public string GetInteractPrompt()
    {
        if (isInteractionLocked)
            return "Meşgul...";


        return isHeld
            ? $"E - {itemName} bırak"
            : $"E - {itemName} al";
    }


    public void Interact(PlayerInteraction player)
    {
        if (isInteractionLocked)
            return;


        if (!isHeld)
        {
            PickUp(player);
        }
        else
        {
            Drop(player);
        }
    }


    // =========================================================
    // ETKİLEŞİM KİLİDİ
    // =========================================================

    public void SetInteractionLocked(bool locked)
    {
        isInteractionLocked = locked;
    }


    public bool IsInteractionLocked =>
        isInteractionLocked;


    // =========================================================
    // PICK UP
    // =========================================================

    private void PickUp(PlayerInteraction player)
    {
        if (isInteractionLocked)
            return;


        KettleHeatController kettleHeat =
            GetComponent<KettleHeatController>();


        if (kettleHeat != null &&
            kettleHeat.IsHeating)
        {
            Debug.Log(
                "Kettle şu anda ısınıyor, alınamaz."
            );

            return;
        }


        if (isAttachedToObject)
        {
            DetachFromObject();
        }


        holdPoint =
            isLeftHandOnly
                ? player.LeftHoldPoint
                : player.HoldPoint;


        rb.isKinematic = true;
        rb.useGravity = false;

        col.enabled = false;

        isHeld = true;


        if (isLeftHandOnly)
        {
            player.SetLeftHeldItem(this);
        }
        else
        {
            player.SetHeldItem(this);
        }

        OnPickedUp?.Invoke(this);
    }


    // =========================================================
    // DROP
    // =========================================================

    public void Drop(PlayerInteraction player)
    {
        if (isInteractionLocked)
            return;


        rb.isKinematic = false;
        rb.useGravity = true;

        col.enabled = true;

        isHeld = false;

        holdPoint = null;
        hasPositionOverride = false;


        if (isLeftHandOnly)
        {
            player.SetLeftHeldItem(null);
        }
        else
        {
            player.SetHeldItem(null);
        }
    }


    // =========================================================
    // COUNTER
    // =========================================================

    public void PlaceOnCounter(
        Vector3 worldPosition)
    {
        transform.position =
            worldPosition +
            Vector3.up *
            surfaceYOffset;


        transform.rotation =
            uprightRotation;


        rb.isKinematic = true;
        rb.useGravity = false;

        col.enabled = true;

        isHeld = false;

        holdPoint = null;
        hasPositionOverride = false;
    }


    // =========================================================
    // TAM KONUMA YERLEŞTİR
    // =========================================================

    public void PlaceAtExact(
        Vector3 worldPosition,
        Quaternion worldRotation)
    {
        transform.position =
            worldPosition;

        transform.rotation =
            worldRotation;


        rb.isKinematic = true;
        rb.useGravity = false;

        col.enabled = true;

        isHeld = false;

        holdPoint = null;
        hasPositionOverride = false;
    }


    // =========================================================
    // FORCE PICKUP
    // =========================================================

    public void ForcePickUp(
        PlayerInteraction player)
    {
        if (isInteractionLocked)
            return;


        KettleHeatController kettleHeat =
            GetComponent<KettleHeatController>();


        if (kettleHeat != null &&
            kettleHeat.IsHeating)
        {
            Debug.Log(
                "Kettle şu anda ısınıyor, alınamaz."
            );

            return;
        }


        if (isAttachedToObject)
        {
            DetachFromObject();
        }


        holdPoint =
            isLeftHandOnly
                ? player.LeftHoldPoint
                : player.HoldPoint;


        rb.isKinematic = true;
        rb.useGravity = false;

        col.enabled = false;

        isHeld = true;


        if (isLeftHandOnly)
        {
            player.SetLeftHeldItem(this);
        }
        else
        {
            player.SetHeldItem(this);
        }

        OnPickedUp?.Invoke(this);
    }


    // =========================================================
    // DOCK
    // =========================================================

    public void DockAt(
        Vector3 worldPosition,
        Vector3 extraRotationEuler)
    {
        transform.position =
            worldPosition;


        transform.rotation =
            uprightRotation *
            Quaternion.Euler(
                extraRotationEuler
            );


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
        if (parent == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: " +
                "AttachToObject parent null!"
            );

            return;
        }


        transform.position =
            worldPosition;

        transform.rotation =
            worldRotation;


        transform.SetParent(parent);


        attachedParent =
            parent;

        isAttachedToObject = true;


        rb.isKinematic = true;
        rb.useGravity = false;

        col.enabled = true;

        isHeld = false;

        holdPoint = null;
        hasPositionOverride = false;
    }


    // =========================================================
    // OBJEDEN AYIR
    // =========================================================

    public void DetachFromObject()
    {
        if (!isAttachedToObject)
            return;


        Transform oldParent =
            attachedParent;


        if (oldParent != null)
        {
            CupLidReceiver cupLidReceiver =
                oldParent.GetComponent<CupLidReceiver>();


            if (cupLidReceiver != null)
            {
                cupLidReceiver.DetachLid(this);
            }
        }


        Vector3 worldPosition =
            transform.position;

        Quaternion worldRotation =
            transform.rotation;


        transform.SetParent(null);


        transform.position =
            worldPosition;

        transform.rotation =
            worldRotation;


        attachedParent = null;

        isAttachedToObject = false;


        rb.isKinematic = true;
        rb.useGravity = false;

        col.enabled = true;

        isHeld = false;

        holdPoint = null;
        hasPositionOverride = false;
    }


    // =========================================================
    // EL POZİSYON OVERRIDE
    // =========================================================

    public void OverrideHeldPosition(
        Vector3 worldPos,
        Quaternion worldRot)
    {
        hasPositionOverride = true;

        overridePosition =
            worldPos;

        overrideRotation =
            worldRot;
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
        if (isAttachedToObject)
        {
            DetachFromObject();
        }


        transform.position =
            originalWorldPosition;

        transform.rotation =
            uprightRotation;


        rb.isKinematic = true;
        rb.useGravity = false;

        col.enabled = true;

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
            targetPosition =
                overridePosition;

            targetRotation =
                overrideRotation;
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
                Time.deltaTime *
                holdSmoothSpeed
            );


        transform.rotation =
            Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime *
                holdSmoothSpeed
            );
    }


    // =========================================================
    // GETTERS
    // =========================================================

    public bool IsHeld =>
        isHeld;


    public string ItemName =>
        itemName;


    public bool IsLeftHandOnly =>
        isLeftHandOnly;
}