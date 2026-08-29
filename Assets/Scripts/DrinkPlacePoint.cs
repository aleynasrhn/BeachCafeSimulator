using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DrinkPlacePoint : MonoBehaviour, IInteractable
{
    [Header("Kahve Yerleşimi")]
    [SerializeField] private Vector3 positionOffset = Vector3.zero;
    [SerializeField] private Vector3 rotationOffset = Vector3.zero;

    public string GetInteractPrompt()
    {
        return "E - Kahveyi masaya bırak";
    }

    public void Interact(PlayerInteraction player)
    {
        PickupItem held = player.GetHeldItem();

        bool fromLeftHand = false;

        if (held == null)
        {
            held = player.GetLeftHeldItem();
            fromLeftHand = true;
        }

        if (held == null)
            return;

        DrinkRecipe recipe = held.GetComponent<DrinkRecipe>();

        if (recipe == null)
            return;

        CoffeeType? coffeeType = recipe.DetermineCoffeeType();

        if (coffeeType == null)
        {
            Debug.Log("Bu kahve henüz tamamlanmamış.");
            return;
        }

        Vector3 placePosition =
            transform.position +
            transform.TransformDirection(positionOffset);

        held.DockAt(
            placePosition,
            rotationOffset
        );

        if (fromLeftHand)
            player.SetLeftHeldItem(null);
        else
            player.SetHeldItem(null);

        Debug.Log(
            $"Kahve masaya bırakıldı: {coffeeType.Value} - {recipe.Size}"
        );
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            0.04f
        );
    }
}