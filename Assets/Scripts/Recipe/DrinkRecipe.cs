using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Small / Medium / Large kağıt bardaklarının boyutunu
/// ve içine eklenen malzemeleri takip eder.
///
/// Ayrıca:
/// - Espresso shot sayısını
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
    private bool hasFrothedMilk = false;
    private bool hasHotWater = false;


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


    public bool HasMilk =>
        hasMilk;


    public bool HasFrothedMilk =>
        hasFrothedMilk;


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
        hasFrothedMilk ||
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

    public void AddMilk()
    {
        hasMilk = true;
    }


    // =========================================================
    // KÖPÜKLÜ SÜT
    // =========================================================

    public void AddFrothedMilk()
    {
        hasFrothedMilk = true;
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
        // CAPPUCCINO
        // -----------------------------------------------------

        if (hasEspresso &&
            hasFrothedMilk &&
            !hasMilk &&
            !hasHotWater)
        {
            return CoffeeType.Cappuccino;
        }


        // -----------------------------------------------------
        // LATTE
        // -----------------------------------------------------

        if (hasEspresso &&
            hasMilk &&
            !hasFrothedMilk &&
            !hasHotWater)
        {
            return CoffeeType.Latte;
        }


        // -----------------------------------------------------
        // AMERICANO
        // -----------------------------------------------------

        if (hasEspresso &&
            hasHotWater &&
            !hasMilk &&
            !hasFrothedMilk)
        {
            return CoffeeType.Americano;
        }


        // -----------------------------------------------------
        // ESPRESSO
        // -----------------------------------------------------

        if (hasEspresso &&
            !hasMilk &&
            !hasFrothedMilk &&
            !hasHotWater)
        {
            return CoffeeType.Espresso;
        }


        return null;
    }


    // =========================================================
    // TARİFİ SIFIRLA
    // =========================================================

    public void ResetRecipe()
    {
        hasEspresso = false;

        hasMilk = false;

        hasFrothedMilk = false;

        hasHotWater = false;


        espressoShotCount = 0;


        addedExtras.Clear();
    }
}