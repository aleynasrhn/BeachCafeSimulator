using UnityEngine;

public enum PourSourceType { Espresso, Milk }

/// <summary>
/// İki yerde kullanılır:
/// 1) Espresso shot bardağının (espressocup, CupDock'ta duran) kendi objesine ekle,
///    Source Type = Espresso yap.
/// 2) Milk pitcher'ın kendi objesine ekle, Source Type = Milk yap.
///
/// Oyuncu elinde bir kağıt bardak (DrinkRecipe'i olan) varken buraya bakıp E'yi
/// basılı tutunca, içerik kağıt bardağa aktarılır.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PourSource : MonoBehaviour, IHoldInteractable
{
    [SerializeField] private PourSourceType sourceType;
    [SerializeField] private float holdDuration = 1.5f;

    [Tooltip("Espresso için: bu objenin kendi PickupItem'ı (HasEspresso kontrolü için). Milk için boş bırakabilirsin.")]
    [SerializeField] private PickupItem sourcePickupItem;

    [Tooltip("Milk için: bu objenin kendi MilkFiller'ı. Espresso için boş bırakabilirsin.")]
    [SerializeField] private MilkFiller sourceMilkFiller;

    public float HoldDuration => holdDuration;

    public string GetHoldPrompt()
    {
        return sourceType == PourSourceType.Espresso ? "E'ye basılı tut (Espresso Dök)" : "E'ye basılı tut (Süt Dök)";
    }

    public bool CanStartHold(PlayerInteraction player)
    {
        PickupItem held = player.GetHeldItem();
        if (held == null) return false;

        DrinkRecipe recipe = held.GetComponent<DrinkRecipe>();
        if (recipe == null) return false; // elindeki şey kağıt bardak değil

        if (sourceType == PourSourceType.Espresso)
            return sourcePickupItem != null && sourcePickupItem.HasEspresso;
        else
            return sourceMilkFiller != null && sourceMilkFiller.HasMilk;
    }

    public void OnHoldProgress(PlayerInteraction player, float progress01)
    {
        // Özel bir animasyon gerekmiyor şimdilik
    }

    public void OnHoldComplete(PlayerInteraction player)
    {
        PickupItem held = player.GetHeldItem();
        if (held == null) return;

        DrinkRecipe recipe = held.GetComponent<DrinkRecipe>();
        if (recipe == null) return;

        if (sourceType == PourSourceType.Espresso)
        {
            recipe.AddEspresso();
        }
        else
        {
            if (sourceMilkFiller.IsFrothed)
                recipe.AddFrothedMilk();
            else
                recipe.AddMilk();
        }
    }
}