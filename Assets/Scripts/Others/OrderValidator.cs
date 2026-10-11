using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Müşterinin istediği Order ile
/// oyuncunun hazırladığı DrinkRecipe'i karşılaştırır.
///
/// Kontrol edilenler:
/// - Sütün ısısı (Latte: 3-4 sn, Cappuccino: 5-6 sn)
/// - Kahve türü
/// - Bardak boyutu
/// - Espresso shot sayısı
/// - Ekstralar
///
/// "Ekstra Espresso" fiziksel bir ekstra olarak değil,
/// ekstra espresso shot olarak değerlendirilir.
/// </summary>
public static class OrderValidator
{
    // =========================================================
    // ANA KONTROL
    // =========================================================

    public static bool Validate(
        Order order,
        DrinkRecipe recipe,
        out string reason)
    {
        reason = "";


        // -----------------------------------------------------
        // NULL KONTROL
        // -----------------------------------------------------

        if (order == null)
        {
            reason = "Müşteri siparişi bulunamadı.";
            return false;
        }


        if (recipe == null)
        {
            reason = "Hazırlanmış kahve bulunamadı.";
            return false;
        }


        // -----------------------------------------------------
        // SÜT ISISI (Latte / Cappuccino)
        // -----------------------------------------------------

        if (!IsMilkHeatCorrect(
            order.coffeeType,
            recipe,
            out string heatReason))
        {
            reason = heatReason;
            return false;
        }


        // -----------------------------------------------------
        // KAHVE TÜRÜ
        // -----------------------------------------------------

        CoffeeType? preparedCoffeeType =
            recipe.DetermineCoffeeType();


        if (!preparedCoffeeType.HasValue)
        {
            reason = "Hazırlanan kahve geçerli bir kahve değil.";
            return false;
        }


        if (preparedCoffeeType.Value != order.coffeeType)
        {
            reason =
                $"Yanlış kahve. " +
                $"Beklenen: {order.coffeeType}, " +
                $"Hazırlanan: {preparedCoffeeType.Value}.";

            return false;
        }


        // -----------------------------------------------------
        // BOYUT
        // -----------------------------------------------------

        if (recipe.Size != order.size)
        {
            reason =
                $"Yanlış boyut. " +
                $"Beklenen: {order.size}, " +
                $"Hazırlanan: {recipe.Size}.";

            return false;
        }


        // -----------------------------------------------------
        // BEKLENEN SHOT
        // -----------------------------------------------------

        int expectedShotCount =
            GetExpectedShotCount(order);


        // -----------------------------------------------------
        // HAZIRLANAN SHOT
        // -----------------------------------------------------

        int preparedShotCount =
            recipe.EspressoShotCount;


        if (preparedShotCount != expectedShotCount)
        {
            reason =
                $"Yanlış espresso miktarı. " +
                $"Beklenen: {expectedShotCount} shot, " +
                $"Hazırlanan: {preparedShotCount} shot.";

            return false;
        }


        // -----------------------------------------------------
        // EKSTRALAR
        // -----------------------------------------------------

        if (!AreExtrasCorrect(
            order.requestedExtras,
            recipe.AddedExtras))
        {
            reason = "Ekstralar siparişle uyuşmuyor.";
            return false;
        }


        // -----------------------------------------------------
        // HER ŞEY DOĞRU
        // -----------------------------------------------------

        reason =
            "Sipariş doğru hazırlandı.";

        return true;
    }


    // =========================================================
    // SÜT ISISI DOĞRU MU?
    // =========================================================

    private static bool IsMilkHeatCorrect(
        CoffeeType orderedType,
        DrinkRecipe recipe,
        out string reason)
    {
        reason = "";


        // Sadece sütlü kahvelerde kontrol edilir.
        // Süt yoksa ya da sipariş sütlü değilse,
        // kahve türü kontrolü zaten yakalar.
        if (orderedType != CoffeeType.Latte &&
            orderedType != CoffeeType.Cappuccino)
        {
            return true;
        }


        if (!recipe.HasMilk)
            return true;


        MilkHeatState actual =
            recipe.MilkHeat;


        bool isAcceptable =
            orderedType == CoffeeType.Latte
                ? MilkHeatRules.IsLatteHeat(actual)
                : MilkHeatRules.IsCappuccinoHeat(actual);


        if (isAcceptable)
            return true;


        string coffeeName =
            orderedType.ToString();


        if (actual == MilkHeatState.Burnt)
        {
            reason = "Süt yanmış.";
            return false;
        }


        if (actual == MilkHeatState.Cold)
        {
            reason = "Süt hiç ısıtılmamış.";
            return false;
        }


        // Kabul edilen aralığın en düşük değeri.
        MilkHeatState minAccepted =
            orderedType == CoffeeType.Latte
                ? MilkHeatState.LatteReady
                : MilkHeatState.CappuccinoOkay;


        // Enum sırası ısıya göre dizili:
        // actual < minAccepted → yeterince ısınmamış
        // aksi halde            → fazla ısınmış
        if (actual < minAccepted)
        {
            reason =
                $"Süt {coffeeName} için yeterince ısıtılmamış.";
        }
        else
        {
            reason =
                $"Süt {coffeeName} için fazla ısıtılmış.";
        }

        return false;
    }


    // =========================================================
    // BEKLENEN SHOT SAYISI
    // =========================================================

    private static int GetExpectedShotCount(
        Order order)
    {
        // -----------------------------------------------------
        // ESPRESSO
        // -----------------------------------------------------

        if (order.coffeeType ==
            CoffeeType.Espresso)
        {
            return
                order.espressoShot ==
                EspressoShotButtonUI.ShotType.Double
                    ? 2
                    : 1;
        }


        // -----------------------------------------------------
        // NORMAL KAHVELER
        // -----------------------------------------------------

        // Normal kahve:
        // Ekstra Espresso yoksa 1 shot.
        //
        // Ekstra Espresso varsa 2 shot.

        if (HasExtraEspresso(
            order.requestedExtras))
        {
            return 2;
        }


        return 1;
    }


    // =========================================================
    // EXTRA ESPRESSO VAR MI?
    // =========================================================

    private static bool HasExtraEspresso(
        List<string> extras)
    {
        if (extras == null)
            return false;


        return extras.Contains(
            "Ekstra Espresso"
        );
    }


    // =========================================================
    // EKSTRA KONTROLÜ
    // =========================================================

    private static bool AreExtrasCorrect(
        List<string> requestedExtras,
        IReadOnlyList<string> preparedExtras)
    {
        int requestedCount =
            CountRealExtras(
                requestedExtras
            );


        int preparedCount =
            preparedExtras != null
                ? preparedExtras.Count
                : 0;


        if (requestedCount != preparedCount)
            return false;


        // -----------------------------------------------------
        // HER İSTENEN EXTRA VAR MI?
        // -----------------------------------------------------

        if (requestedExtras != null)
        {
            foreach (string requestedExtra
                     in requestedExtras)
            {
                // Ekstra Espresso burada kontrol edilmez.
                // Çünkü shot sayısı olarak kontrol edildi.
                if (requestedExtra ==
                    "Ekstra Espresso")
                {
                    continue;
                }


                if (preparedExtras == null ||
                    !preparedExtras.Contains(
                        requestedExtra))
                {
                    return false;
                }
            }
        }


        // -----------------------------------------------------
        // HAZIRLANAN FAZLADAN EXTRA EKLEMİŞ Mİ?
        // -----------------------------------------------------

        if (preparedExtras != null)
        {
            foreach (string preparedExtra
                     in preparedExtras)
            {
                if (requestedExtras == null ||
                    !requestedExtras.Contains(
                        preparedExtra))
                {
                    return false;
                }
            }
        }


        return true;
    }


    // =========================================================
    // GERÇEK EXTRA SAYISI
    // =========================================================

    private static int CountRealExtras(
        List<string> extras)
    {
        if (extras == null)
            return 0;


        int count = 0;


        foreach (string extra in extras)
        {
            // Ekstra Espresso shot üzerinden kontrol edildiği
            // için gerçek ekstra listesine dahil edilmez.
            if (extra ==
                "Ekstra Espresso")
            {
                continue;
            }


            count++;
        }


        return count;
    }
}