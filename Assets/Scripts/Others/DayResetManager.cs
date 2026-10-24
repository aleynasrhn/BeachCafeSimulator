using UnityEngine;

/// <summary>
/// Gün geçişinde (siyah ekranın arkasında) sahneyi sıfırlar:
/// - Tuvalet kabinleri + ortak alan + kafe zemini: tüm kirler yok olur
/// - Masalardaki (müşteriye servis edilmiş) kullanılmış bardaklar yok edilir
/// - Elde tutulmayan alet/ekipman (portafilter, shot bardağı, kettle,
///   tamper vb.) kendi orijinal konumuna döner, telve/espresso temizlenir
/// - Çöp poşetleri ve atılabilir kutular olduğu yerde kalır
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
            Object.FindObjectsByType<DrinkPlacePoint>(
                FindObjectsSortMode.None
            );

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
            Object.FindObjectsByType<AreaLitterZone>(
                FindObjectsSortMode.None
            );

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
    // Masadaki bardaklar üst adımda Destroy ile işaretlendi, kare
    // sonunda silinecekler; burada konumlarının değişmesi önemsiz.
    //
    // Çöp poşetleri ve atılabilir kutular alet değildir: yere
    // bıraktıysan olduğu yerde kalır.
    //
    // =========================================================

    private static void ReturnToolsToOriginalPosition()
    {
        PickupItem[] items =
            Object.FindObjectsByType<PickupItem>(
                FindObjectsSortMode.None
            );

        foreach (PickupItem item in items)
        {
            if (item == null)
                continue;

            if (item.IsHeld)
            {
                // Oyuncunun elindeyse dokunma.
                continue;
            }

            if (item.GetComponent<TrashBagItem>() != null ||
                item.GetComponent<TrashableItem>() != null)
            {
                continue;
            }

            item.ReturnToOriginalPosition();
            item.EmptyGroundCoffee();
            item.EmptyEspresso();
        }
    }
}