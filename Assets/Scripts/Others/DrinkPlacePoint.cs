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

    [Header("Yedek Bahşiş Aralığı (TipManager yoksa)")]
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
    // BAHŞİŞ OLUŞTUR (YEDEK: RASTGELE)
    // =========================================================

    public void SpawnTip()
    {
        SpawnTip(
            Random.Range(
                tipMinAmount,
                tipMaxAmount
            )
        );
    }

    // =========================================================
    // BAHŞİŞ OLUŞTUR (HESAPLANMIŞ MİKTAR)
    // =========================================================

    public void SpawnTip(float amount)
    {
        if (amount <= 0f)
            return;

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
    // YENİ GÜN: MASADAKİ BARDAĞI YOK ET
    // =========================================================

    public void ClearCupForNewDay()
    {
        if (currentCupOnTable != null)
        {
            currentCupOnTable.OnPickedUp -=
                HandleTableCupPickedUp;

            Destroy(currentCupOnTable.gameObject);

            currentCupOnTable = null;
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

        DrinkRecipe recipe =
            held.GetComponent<DrinkRecipe>();

        if (recipe == null)
        {
            Debug.Log(
                "Bu item bir DrinkRecipe içermiyor."
            );

            return;
        }

        CoffeeType? coffeeType =
            recipe.DetermineCoffeeType();

        if (!coffeeType.HasValue)
        {
            Debug.Log(
                "Bu kahve henüz tamamlanmamış."
            );

            return;
        }

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

        if (currentCupOnTable != null)
        {
            Debug.Log(
                $"{gameObject.name}: " +
                "Masada zaten bir bardak var."
            );

            return;
        }

        bool isCorrect =
            OrderValidator.Validate(
                customer.CustomerOrder,
                recipe,
                out string reason
            );

        Vector3 placePosition =
            transform.position +
            transform.TransformDirection(
                positionOffset
            );

        held.DockAt(
            placePosition,
            rotationOffset
        );

        if (fromLeftHand)
        {
            player.SetLeftHeldItem(null);
        }
        else
        {
            player.SetHeldItem(null);
        }

        currentCupOnTable =
            held;

        held.OnPickedUp +=
            HandleTableCupPickedUp;

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

        Debug.Log(
            $"Sipariş yanlış! " +
            $"{customer.gameObject.name}: " +
            $"{reason}"
        );

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