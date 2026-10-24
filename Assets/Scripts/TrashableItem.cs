using UnityEngine;

/// <summary>
/// Çöp kovasına atılabilen ama başka bir özel bileşeni (DrinkRecipe,
/// LidItem, LitterMarker) olmayan eşyalara eklenir. Örneğin boş
/// kutular. Bu bileşeni taşıyan PickupItem kovaya atılabilir.
/// </summary>
[RequireComponent(typeof(PickupItem))]
public class TrashableItem : MonoBehaviour
{
}