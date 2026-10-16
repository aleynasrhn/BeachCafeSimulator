using UnityEngine;

/// <summary>
/// Elle alınabilen kir objelerine (kağıt vb.) eklenir.
/// PickupItem zaten var olan al/bırak sistemini sağlıyor,
/// bu script sadece "bu hangi bölgenin kiri" bilgisini taşır.
/// </summary>
[RequireComponent(typeof(PickupItem))]
public class LitterMarker : MonoBehaviour
{
    private AreaLitterZone zone;

    public AreaLitterZone Zone => zone;

    private void Awake()
    {
        zone = GetComponentInParent<AreaLitterZone>();

        if (zone == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Üstünde AreaLitterZone bulunamadı!"
            );
        }
    }
}