using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Small / Medium / Large kağıt bardaklarının boyutunu
/// ve içine eklenen malzemeleri takip eder.
///
/// Ayrıca:
/// - Espresso shot sayısını
/// - Sütün ısı durumunu
/// - Ekstra malzemeleri
/// takip eder.
/// </summary>
[RequireComponent(typeof(PickupItem))]
public class DrinkRecipe : MonoBehaviour
{
    [Tooltip("Bu bardağın boyutu")]
    [SerializeField] private CupSize size;


    // =========================================================
    // İÇERİK DURUMLARI
    // =========================================================

    private bool hasEspresso = false;
    private bool hasMilk = false;
    private bool hasHotWater = false;

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


    public int EspressoShotCount =>
        espressoShotCount;


    public IReadOnlyList<string> AddedExtras =>
        addedExtras;


    public bool HasAnyExtra =>
        addedExtras.Count > 0;


    public bool HasAnyContent =>
        hasEspresso ||
        hasMilk ||
        hasHotWater;


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

        milkHeat = MilkHeatState.Cold;


        espressoShotCount = 0;


        addedExtras.Clear();
    }
}