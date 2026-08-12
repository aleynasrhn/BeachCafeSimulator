using UnityEngine;

/// <summary>
/// Small/Medium/Large kağıt bardak prefab'larının HER BİRİNE, PickupItem'ın YANINA eklenir.
/// Bardağın boyutunu (Inspector'dan) ve içine ne eklendiğini (PourSource'lar aracılığıyla)
/// takip eder, sonunda hangi kahve tarifine denk geldiğini hesaplar.
/// </summary>
[RequireComponent(typeof(PickupItem))]
public class DrinkRecipe : MonoBehaviour
{
    [Tooltip("Bu bardağın boyutu - Small/Medium/Large prefab'ına göre Inspector'dan ayarla")]
    [SerializeField] private CupSize size;

    private bool hasEspresso = false;
    private bool hasMilk = false;
    private bool hasFrothedMilk = false;
    private bool hasHotWater = false;

    public CupSize Size => size;
    public bool HasAnyContent => hasEspresso || hasMilk || hasFrothedMilk || hasHotWater;

    public void AddEspresso() => hasEspresso = true;
    public void AddMilk() => hasMilk = true;
    public void AddFrothedMilk() => hasFrothedMilk = true;
    public void AddHotWater() => hasHotWater = true;

    /// <summary>
    /// Şu ana kadar eklenenlere göre hangi kahve olduğunu hesaplar.
    /// Hiçbir bilinen tarife uymuyorsa null döner (örn. sadece süt, kahve yok).
    /// </summary>
    public CoffeeType? DetermineCoffeeType()
    {
        if (hasEspresso && hasFrothedMilk && !hasMilk && !hasHotWater)
            return CoffeeType.Cappuccino;

        if (hasEspresso && hasMilk && !hasFrothedMilk && !hasHotWater)
            return CoffeeType.Latte;

        if (hasEspresso && hasHotWater && !hasMilk && !hasFrothedMilk)
            return CoffeeType.Americano;

        if (hasEspresso && !hasMilk && !hasFrothedMilk && !hasHotWater)
            return CoffeeType.Espresso;

        return null; // tanınmayan/eksik kombinasyon
    }

    /// <summary>Bardak teslim edildikten/tüketildikten sonra sıfırlamak için (ileride yeniden kullanım varsa).</summary>
    public void ResetRecipe()
    {
        hasEspresso = false;
        hasMilk = false;
        hasFrothedMilk = false;
        hasHotWater = false;
    }
}