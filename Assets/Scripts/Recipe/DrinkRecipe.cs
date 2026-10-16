using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Small / Medium / Large kağıt bardaklarının boyutunu
/// ve içine eklenen malzemeleri takip eder.
///
/// Ayrıca:
/// - Espresso shot sayısını
/// - Sütün ısı durumunu
/// - Filtre kahve olup olmadığını
/// - Ekstra malzemeleri
/// takip eder.
/// </summary>
[RequireComponent(typeof(PickupItem))]
public class DrinkRecipe : MonoBehaviour
{
    [Tooltip("Bu bardağın boyutu")]
    [SerializeField] private CupSize size;


    // Mocha için gereken çikolata şurubu ekstrasının adı.
    // Sipariş / ExtraButton tarafındaki yazımla birebir aynı olmalı.
    private const string ChocolateSyrupName =
        "Çikolata Şurubu";


    // =========================================================
    // İÇERİK DURUMLARI
    // =========================================================

    private bool hasEspresso = false;
    private bool hasMilk = false;
    private bool hasHotWater = false;

    // Bardakta sürahiden dökülmüş filtre kahve var mı?
    private bool hasFilterCoffee = false;

    // Bardağa dökülen sütün ısı durumu.
    private MilkHeatState milkHeat =
        MilkHeatState.Cold;


    // =========================================================
    // ESPRESSO SHOT
    // =========================================================

    private int espressoShotCount = 0;


    // =========================================================
    // EKSTRALAR
    // =========================================================

    private readonly List<string> addedExtras =
        new List<string>();


    // =========================================================
    // GETTERS
    // =========================================================

    public CupSize Size =>
        size;


    public bool HasEspresso =>
        hasEspresso;


    // Bardakta herhangi bir süt var mı? (soğuk veya sıcak)
    public bool HasMilk =>
        hasMilk;


    // Eski scriptler bozulmasın diye duruyor:
    // ısıtılmış süt varsa true.
    public bool HasFrothedMilk =>
        hasMilk &&
        milkHeat != MilkHeatState.Cold;


    public MilkHeatState MilkHeat =>
        milkHeat;


    public bool HasHotWater =>
        hasHotWater;


    public bool HasFilterCoffee =>
        hasFilterCoffee;


    public int EspressoShotCount =>
        espressoShotCount;


    public IReadOnlyList<string> AddedExtras =>
        addedExtras;


    public bool HasAnyExtra =>
        addedExtras.Count > 0;


    public bool HasAnyContent =>
        hasEspresso ||
        hasMilk ||
        hasHotWater ||
        hasFilterCoffee;


    // =========================================================
    // ESPRESSO
    // =========================================================

    public void AddEspresso()
    {
        hasEspresso = true;

        // Bir espresso eklendiğinde
        // shot sayısını 1 artır.
        espressoShotCount++;
    }


    // =========================================================
    // FİLTRE KAHVE
    // =========================================================

    public void AddFilterCoffee()
    {
        hasFilterCoffee = true;
    }


    // =========================================================
    // SÜT
    // =========================================================

    // Parametre verilmezse soğuk süt kabul edilir
    // (örneğin süt kutusundan direkt dökülen süt).
    public void AddMilk(
        MilkHeatState heat = MilkHeatState.Cold)
    {
        hasMilk = true;

        milkHeat = heat;
    }


    // Eski çağrılar için. Cappuccino ısısında süt ekler.
    public void AddFrothedMilk()
    {
        AddMilk(
            MilkHeatState.CappuccinoReady
        );
    }


    // =========================================================
    // SICAK SU
    // =========================================================

    public void AddHotWater()
    {
        hasHotWater = true;
    }


    // =========================================================
    // EXTRA EKLE
    // =========================================================

    public void AddExtra(string extraName)
    {
        if (string.IsNullOrWhiteSpace(extraName))
            return;


        // Aynı ekstranın iki kere eklenmesini
        // şimdilik engelliyoruz.
        if (addedExtras.Contains(extraName))
            return;


        addedExtras.Add(
            extraName
        );
    }


    // =========================================================
    // EXTRA VAR MI?
    // =========================================================

    public bool HasExtra(string extraName)
    {
        if (string.IsNullOrWhiteSpace(extraName))
            return false;


        return addedExtras.Contains(
            extraName
        );
    }


    // =========================================================
    // EXTRA ÇIKAR
    // =========================================================

    public void RemoveExtra(string extraName)
    {
        if (string.IsNullOrWhiteSpace(extraName))
            return;


        addedExtras.Remove(
            extraName
        );
    }


    // =========================================================
    // KAHVE TÜRÜNÜ BELİRLE
    // =========================================================

    public CoffeeType? DetermineCoffeeType()
    {
        // -----------------------------------------------------
        // FİLTRE KAHVE
        //
        // Bardakta filtre kahve varsa başka hiçbir şey
        // (espresso, süt, su) olmamalı. Karışmışsa geçersiz.
        // Bu kontrol en başta olmalı, yoksa espresso+filtre
        // karışımı "Espresso" sayılır.
        // -----------------------------------------------------

        if (hasFilterCoffee)
        {
            if (!hasEspresso &&
                !hasMilk &&
                !hasHotWater)
            {
                return CoffeeType.Filter;
            }

            return null;
        }


        // -----------------------------------------------------
        // CAPPUCCINO (süt 5-6 sn ısıtılmış olmalı)
        // -----------------------------------------------------

        if (hasEspresso &&
            hasMilk &&
            MilkHeatRules.IsCappuccinoHeat(milkHeat) &&
            !hasHotWater)
        {
            return CoffeeType.Cappuccino;
        }


        // -----------------------------------------------------
        // MOCHA (latte ısısında süt + çikolata şurubu)
        //
        // Latte'den ÖNCE kontrol edilmeli, yoksa
        // çikolata şuruplu bardak latte sayılır.
        // -----------------------------------------------------

        if (hasEspresso &&
            hasMilk &&
            MilkHeatRules.IsLatteHeat(milkHeat) &&
            !hasHotWater &&
            HasExtra(ChocolateSyrupName))
        {
            return CoffeeType.Mocha;
        }


        // -----------------------------------------------------
        // LATTE (süt 3-4 sn ısıtılmış olmalı)
        // -----------------------------------------------------

        if (hasEspresso &&
            hasMilk &&
            MilkHeatRules.IsLatteHeat(milkHeat) &&
            !hasHotWater)
        {
            return CoffeeType.Latte;
        }


        // -----------------------------------------------------
        // AMERICANO
        // -----------------------------------------------------

        if (hasEspresso &&
            hasHotWater &&
            !hasMilk)
        {
            return CoffeeType.Americano;
        }


        // -----------------------------------------------------
        // ESPRESSO
        // -----------------------------------------------------

        if (hasEspresso &&
            !hasMilk &&
            !hasHotWater)
        {
            return CoffeeType.Espresso;
        }


        // Soğuk / yetersiz / fazla ısınmış / yanık süt
        // geçerli bir kahve türü değil.
        return null;
    }


    // =========================================================
    // TARİFİ SIFIRLA
    // =========================================================

    public void ResetRecipe()
    {
        hasEspresso = false;

        hasMilk = false;

        hasHotWater = false;

        hasFilterCoffee = false;

        milkHeat = MilkHeatState.Cold;


        espressoShotCount = 0;


        addedExtras.Clear();
    }


    // =========================================================
    // GEÇİCİ TEST (filtre kahve mekanizması yapılınca kaldır)
    // =========================================================

    // Play modunda, Inspector'da bu component'in başlığına
    // sağ tıkla → "TEST - Filtre Kahve Ekle" seçeneğini kullan.
    [ContextMenu("TEST - Filtre Kahve Ekle")]
    private void DebugAddFilterCoffee()
    {
        AddFilterCoffee();

        Debug.Log(
            $"TEST: {gameObject.name} bardağına filtre kahve eklendi. " +
            $"Tür: {DetermineCoffeeType()}"
        );
    }
}