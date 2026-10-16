using UnityEngine;

/// <summary>
/// Gün geçişinde (siyah ekranın arkasında) sahneyi sıfırlar:
/// - Tuvalet kabinleri + ortak alan + kafe zemini: tüm kirler yok olur
/// - Masalardaki (müşteriye servis edilmiş) kullanılmış bardaklar yok edilir
/// - Elde tutulmayan tüm diğer ekipman (portafilter, shot bardağı,
///   kettle, tamper vb.) kendi orijinal konumuna/rotasyonuna döner
///   ve varsa üzerindeki kahve/espresso telvesi temizlenir
///
/// Oyuncunun pozisyonuna/rotasyonuna HİÇ dokunulmaz.
/// </summary>
public static class DayResetManager
{
    public static void ResetSceneForNewDay()
    {
        ClearAllTableCups();
        CleanAllLitter();
        ReturnToolsToOriginalPosition();

        Debug.Log("DayResetManager: Sahne yeni gün için sıfırlandı.");
    }

    // =========================================================
    // 1) MASALARDAKİ BARDAKLARI YOK ET
    // =========================================================

    private static void ClearAllTableCups()
    {
        DrinkPlacePoint[] points =
            Object.FindObjectsOfType<DrinkPlacePoint>();

        foreach (DrinkPlacePoint point in points)
        {
            if (point != null)
            {
                point.ClearCupForNewDay();
            }
        }
    }

    // =========================================================
    // 2) TÜM KİRLERİ TEMİZLE (TUVALET + KAFE ZEMİNİ)
    // =========================================================

    private static void CleanAllLitter()
    {
        AreaLitterZone[] areaZones =
            Object.FindObjectsOfType<AreaLitterZone>();

        foreach (AreaLitterZone zone in areaZones)
        {
            zone.CleanAll();
        }

        if (CafeFloorLitterZone.Instance != null)
        {
            CafeFloorLitterZone.Instance.CleanAll();
        }
    }

    // =========================================================
    // 3) EKİPMANI ESKİ KONUMUNA DÖNDÜR
    // =========================================================
    //
    // Masadaki bardaklar üst adımda zaten yok edildi (Unity'de
    // Destroy() çağrısından hemen sonra referans "null" sayılır),
    // bu yüzden burada kalan her PickupItem bir ARAÇ'tır — elde
    // tutulmuyorsa kendi başlangıç konumuna/rotasyonuna döner ve
    // üzerindeki kahve telvesi/espresso varsa temizlenir.
    //
    // =========================================================

    private static void ReturnToolsToOriginalPosition()
    {
        PickupItem[] items =
            Object.FindObjectsOfType<PickupItem>();

        foreach (PickupItem item in items)
        {
            if (item == null)
                continue;

            if (item.IsHeld)
            {
                // Oyuncunun elindeyse dokunma.
                continue;
            }

            item.ReturnToOriginalPosition();
            item.EmptyGroundCoffee();
            item.EmptyEspresso();
        }
    }
}