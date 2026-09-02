using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DrinkPlacePoint : MonoBehaviour, IInteractable
{
    [Header("Kahve Yerleşimi")]
    [SerializeField] private Vector3 positionOffset = Vector3.zero;
    [SerializeField] private Vector3 rotationOffset = Vector3.zero;

    [Header("Müşteri")]
    [SerializeField] private NPCController customer;


    // =========================================================
    // MÜŞTERİ ATA
    // =========================================================

    public void SetCustomer(NPCController npc)
    {
        customer = npc;

        if (customer != null)
        {
            Debug.Log(
                $"{gameObject.name} → " +
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


    // =========================================================
    // MÜŞTERİYİ GETİR
    // =========================================================

    public NPCController GetCustomer()
    {
        return customer;
    }


    // =========================================================
    // PROMPT
    // =========================================================

    public string GetInteractPrompt()
    {
        return "E - Kahveyi masaya bırak";
    }


    // =========================================================
    // INTERACT
    // =========================================================

    public void Interact(PlayerInteraction player)
    {
        if (player == null)
            return;


        // =====================================================
        // OYUNCUNUN ELİNDEKİ BARDAĞI BUL
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
        // DRINK RECIPE KONTROL
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
        // KAHVE TÜRÜ KONTROL
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
        // MÜŞTERİ KONTROLÜ
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
        // SİPARİŞ DOĞRULAMA
        // =====================================================

        bool isCorrect =
            OrderValidator.Validate(
                customer.CustomerOrder,
                recipe,
                out string reason
            );


        if (!isCorrect)
        {
            Debug.Log(
                $"Sipariş yanlış! " +
                $"{customer.gameObject.name}: " +
                $"{reason}"
            );

            return;
        }


        // =====================================================
        // DOĞRU SİPARİŞ
        // =====================================================

        Debug.Log(
            $"Sipariş doğru! " +
            $"{customer.gameObject.name} kahveyi kabul etti."
        );


        // =====================================================
        // MASAYA BIRAK
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


        // El durumunu temizle.
        if (fromLeftHand)
        {
            player.SetLeftHeldItem(null);
        }
        else
        {
            player.SetHeldItem(null);
        }


        // =====================================================
        // TESLİMAT DEBUG
        // =====================================================

        Debug.Log(
            $"Kahve teslim edildi: " +
            $"{coffeeType.Value} - " +
            $"{recipe.Size}"
        );
    }


    // =========================================================
    // GİZMO
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