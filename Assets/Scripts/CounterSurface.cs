using UnityEngine;

/// <summary>
/// Tezgahın (counter) mesh'ine sahip GameObject'e eklenir — collider'ı olan obje olmalı.
/// GameObject'in Layer'ını "Interactable" yap.
///
/// PlacementPoint sistemi yerine geçer: sabit noktalar yerine, oyuncu tezgahın
/// neresine bakıyorsa elindeki item oraya bırakılır.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CounterSurface : MonoBehaviour, IInteractable
{
    [Header("Ayarlar")]
    [Tooltip("Zaten bir item olan noktaya çok yakına bırakmayı engellemek için minimum mesafe")]
    [SerializeField] private float minDistanceBetweenItems = 0.15f;
    [Tooltip("Yerdeki item'ları kontrol etmek için kullanılan layer - Interactable seç")]
    [SerializeField] private LayerMask itemLayer;
    [Tooltip("Coffee grinder, espresso machine gibi tezgah üstündeki sabit makinelerin collider'larının olduğu layer - buraya item bırakılamasın")]
    [SerializeField] private LayerMask obstacleLayer;

    public string GetInteractPrompt()
    {
        return "E - Tezgaha bırak";
    }

    public void Interact(PlayerInteraction player)
    {
        PickupItem held = player.GetHeldItem();
        if (held == null) return; // elinde bir şey yoksa tezgahla etkileşimin anlamı yok (şimdilik)

        Vector3 placePos = player.LastHitPoint;

        if (IsSpotOccupied(placePos))
            return; // çok yakında zaten bir item var, üst üste bindirme

        held.PlaceOnCounter(placePos);
        player.SetHeldItem(null);
    }

    private bool IsSpotOccupied(Vector3 pos)
    {
        Vector3 checkCenter = pos + Vector3.up * 0.05f;

        // 1) Zaten bırakılmış bir item var mı?
        Collider[] nearbyItems = Physics.OverlapSphere(checkCenter, minDistanceBetweenItems, itemLayer);
        foreach (var hitCollider in nearbyItems)
        {
            if (hitCollider.TryGetComponent(out PickupItem item) && !item.IsHeld)
                return true;
        }

        // 2) Grinder, espresso machine gibi sabit bir makineye çarpıyor mu?
        Collider[] nearbyObstacles = Physics.OverlapSphere(checkCenter, minDistanceBetweenItems, obstacleLayer);
        if (nearbyObstacles.Length > 0)
            return true;

        return false;
    }
}