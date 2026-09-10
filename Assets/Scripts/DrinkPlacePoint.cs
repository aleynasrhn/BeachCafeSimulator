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
    // CUSTOMER
    // =========================================================

    public void SetCustomer(NPCController npc)
    {
        customer = npc;

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
    // INTERACTION
    // =========================================================

    public string GetInteractPrompt()
    {
        return "E - Kahveyi masaya bırak";
    }


    public void Interact(PlayerInteraction player)
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
        // DRINK RECIPE KONTROLÜ
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
        // KAHVE TÜRÜ KONTROLÜ
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
        // CUSTOMER KONTROLÜ
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
                $"{customer.gameObject.name} için sipariş bulunamadı."
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
        // YANLIŞ KAHVE
        // =====================================================

        if (!isCorrect)
        {
            Debug.Log(
                $"Sipariş yanlış! " +
                $"{customer.gameObject.name}: {reason}"
            );

            return;
        }


        // =====================================================
        // DOĞRU KAHVE
        // =====================================================

        Debug.Log(
            $"Sipariş doğru! " +
            $"{customer.gameObject.name} kahveyi kabul etti."
        );


        // =====================================================
        // KAHVEYİ MASAYA KOY
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


        if (fromLeftHand)
        {
            player.SetLeftHeldItem(null);
        }
        else
        {
            player.SetHeldItem(null);
        }


        Debug.Log(
            $"Kahve teslim edildi: " +
            $"{coffeeType.Value} - {recipe.Size}"
        );


        // =====================================================
        // NPC İÇME ANİMASYONUNU BAŞLAT
        // =====================================================

        customer.StartDrinkSequence(
            held
        );
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