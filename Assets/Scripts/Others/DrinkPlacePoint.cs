using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DrinkPlacePoint : MonoBehaviour, IInteractable
{
    [Header("Kahve Yerleşimi")]
    [SerializeField] private Vector3 positionOffset = Vector3.zero;
    [SerializeField] private Vector3 rotationOffset = Vector3.zero;

    [Header("Müşteri")]
    [SerializeField] private NPCController customer;

    [Header("Bahşiş")]
    [SerializeField] private TipPickup tipPrefab;
    [SerializeField]
    private Vector3 tipPositionOffset =
        new Vector3(0.15f, 0f, 0f);

    [SerializeField] private float tipMinAmount = 0.5f;
    [SerializeField] private float tipMaxAmount = 1f;

    // =========================================================
    // MASADAKİ EŞYA TAKİBİ
    // =========================================================

    private PickupItem currentCupOnTable;

    private TipPickup currentTip;

    private Collider ownCollider;

    // Masada bardak veya bahşiş varsa true.
    public bool IsOccupiedByItem =>
        currentCupOnTable != null ||
        currentTip != null;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        ownCollider =
            GetComponent<Collider>();
    }

    // =========================================================
    // CUSTOMER
    // =========================================================

    public void SetCustomer(
        NPCController npc)
    {
        customer =
            npc;

        if (customer != null)
        {
            Debug.Log(
                $"{gameObject.name} -> " +
                $"{customer.gameObject.name} müşterisine bağlandı."
            );
        }
        else
        {
            Debug.Log(
                $"{gameObject.name} müşteri bağlantısı temizlendi."
            );
        }
    }

    public NPCController GetCustomer()
    {
        return customer;
    }

    // =========================================================
    // BAHŞİŞ OLUŞTUR
    // =========================================================

    public void SpawnTip()
    {
        if (tipPrefab == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: " +
                "Tip Prefab atanmamış, bahşiş oluşturulamadı!"
            );

            return;
        }

        if (currentTip != null)
        {
            return;
        }

        float amount =
            Random.Range(
                tipMinAmount,
                tipMaxAmount
            );

        Vector3 spawnPosition =
            transform.position +
            transform.TransformDirection(
                tipPositionOffset
            );

        TipPickup tip =
            Instantiate(
                tipPrefab,
                spawnPosition,
                transform.rotation
            );

        tip.Initialize(
            amount,
            this
        );

        currentTip =
            tip;

        // ---------------------------------------------------------
        // ÖNEMLİ:
        // DrinkPlacePoint collider'ı Coin'in önüne geçmesin.
        // ---------------------------------------------------------

        if (ownCollider != null)
        {
            ownCollider.enabled = false;
        }

        Debug.Log(
            $"{gameObject.name}: " +
            $"Bahşiş oluştu ({amount:0.00}$)."
        );
    }

    // =========================================================
    // BAHŞİŞ TEMİZLENDİ
    // =========================================================

    public void ClearTip()
    {
        currentTip =
            null;

        // Bahşiş alındıktan sonra
        // DrinkPlacePoint tekrar kullanılabilir.
        if (ownCollider != null)
        {
            ownCollider.enabled = true;
        }
    }

    // =========================================================
    // INTERACTION PROMPT
    // =========================================================

    public string GetInteractPrompt()
    {
        return "E - Kahveyi masaya bırak";
    }

    // =========================================================
    // INTERACT
    // =========================================================

    public void Interact(
        PlayerInteraction player)
    {
        if (player == null)
            return;

        // =====================================================
        // ELDEN KAHVEYİ AL
        // =====================================================

        PickupItem held =
            player.GetHeldItem();

        bool fromLeftHand = false;

        if (held == null)
        {
            held =
                player.GetLeftHeldItem();

            fromLeftHand = true;
        }

        if (held == null)
            return;

        // =====================================================
        // DRINK RECIPE
        // =====================================================

        DrinkRecipe recipe =
            held.GetComponent<DrinkRecipe>();

        if (recipe == null)
        {
            Debug.Log(
                "Bu item bir DrinkRecipe içermiyor."
            );

            return;
        }

        // =====================================================
        // KAHVE TÜRÜ
        // =====================================================

        CoffeeType? coffeeType =
            recipe.DetermineCoffeeType();

        if (!coffeeType.HasValue)
        {
            Debug.Log(
                "Bu kahve henüz tamamlanmamış."
            );

            return;
        }

        // =====================================================
        // CUSTOMER
        // =====================================================

        if (customer == null)
        {
            Debug.LogWarning(
                $"{gameObject.name} için müşteri bulunamadı."
            );

            return;
        }

        if (customer.CustomerOrder == null)
        {
            Debug.LogWarning(
                $"{customer.gameObject.name} " +
                "için sipariş bulunamadı."
            );

            return;
        }

        // =====================================================
        // MASADA ZATEN BARDAK VAR MI?
        // =====================================================

        if (currentCupOnTable != null)
        {
            Debug.Log(
                $"{gameObject.name}: " +
                "Masada zaten bir bardak var."
            );

            return;
        }

        // =====================================================
        // SİPARİŞ DOĞRULAMA
        // =====================================================

        bool isCorrect =
            OrderValidator.Validate(
                customer.CustomerOrder,
                recipe,
                out string reason
            );

        // =====================================================
        // KAHVEYİ MASAYA KOY
        // DOĞRU / YANLIŞ FARK ETMEZ
        // =====================================================

        Vector3 placePosition =
            transform.position +
            transform.TransformDirection(
                positionOffset
            );

        held.DockAt(
            placePosition,
            rotationOffset
        );

        // =====================================================
        // OYUNCUNUN ELİNİ TEMİZLE
        // =====================================================

        if (fromLeftHand)
        {
            player.SetLeftHeldItem(null);
        }
        else
        {
            player.SetHeldItem(null);
        }

        // =====================================================
        // MASADAKİ BARDAĞI KAYDET
        // =====================================================

        currentCupOnTable =
            held;

        held.OnPickedUp +=
            HandleTableCupPickedUp;

        // =====================================================
        // DOĞRU KAHVE
        // =====================================================

        if (isCorrect)
        {
            Debug.Log(
                $"Sipariş doğru! " +
                $"{customer.gameObject.name} " +
                "kahveyi kabul etti."
            );

            customer.ServeOrder(
                true,
                held
            );

            return;
        }

        // =====================================================
        // YANLIŞ KAHVE
        // =====================================================

        Debug.Log(
            $"Sipariş yanlış! " +
            $"{customer.gameObject.name}: " +
            $"{reason}"
        );

        // Para cezasını burada vermiyoruz.
        // NPC önce kahveyi içecek.
        // Sonuç daha sonra NPC tarafından yönetilecek.

        customer.ServeOrder(
            false,
            held
        );
    }

    // =========================================================
    // MASADAKİ BARDAK GERİ ALINDI
    // =========================================================

    private void HandleTableCupPickedUp(
        PickupItem cup)
    {
        if (cup != currentCupOnTable)
            return;

        currentCupOnTable.OnPickedUp -=
            HandleTableCupPickedUp;

        currentCupOnTable =
            null;
    }

    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (currentCupOnTable != null)
        {
            currentCupOnTable.OnPickedUp -=
                HandleTableCupPickedUp;
        }
    }

    // =========================================================
    // GIZMO
    // =========================================================

    private void OnDrawGizmos()
    {
        Gizmos.color =
            Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            0.04f
        );
    }
}