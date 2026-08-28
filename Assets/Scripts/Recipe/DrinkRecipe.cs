using UnityEngine;

/// <summary>
/// Small/Medium/Large kağıt bardaklarının boyutunu
/// ve içine eklenen malzemeleri takip eder.
/// </summary>
[RequireComponent(typeof(PickupItem))]
public class DrinkRecipe : MonoBehaviour
{
    [Tooltip("Bu bardağın boyutu")]
    [SerializeField] private CupSize size;

    private bool hasEspresso = false;
    private bool hasMilk = false;
    private bool hasFrothedMilk = false;
    private bool hasHotWater = false;

    public CupSize Size => size;

    public bool HasAnyContent =>
        hasEspresso ||
        hasMilk ||
        hasFrothedMilk ||
        hasHotWater;

    public void AddEspresso()
    {
        hasEspresso = true;
    }

    public void AddMilk()
    {
        hasMilk = true;
    }

    public void AddFrothedMilk()
    {
        hasFrothedMilk = true;
    }

    public void AddHotWater()
    {
        hasHotWater = true;
    }

    public CoffeeType? DetermineCoffeeType()
    {
        if (hasEspresso &&
            hasFrothedMilk &&
            !hasMilk &&
            !hasHotWater)
        {
            return CoffeeType.Cappuccino;
        }

        if (hasEspresso &&
            hasMilk &&
            !hasFrothedMilk &&
            !hasHotWater)
        {
            return CoffeeType.Latte;
        }

        if (hasEspresso &&
            hasHotWater &&
            !hasMilk &&
            !hasFrothedMilk)
        {
            return CoffeeType.Americano;
        }

        if (hasEspresso &&
            !hasMilk &&
            !hasFrothedMilk &&
            !hasHotWater)
        {
            return CoffeeType.Espresso;
        }

        return null;
    }

    public void ResetRecipe()
    {
        hasEspresso = false;
        hasMilk = false;
        hasFrothedMilk = false;
        hasHotWater = false;
    }
}