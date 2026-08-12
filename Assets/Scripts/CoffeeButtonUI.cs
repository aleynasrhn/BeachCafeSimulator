using UnityEngine;

/// <summary>
/// Americano/Latte/Cappuccino/Espresso butonlarının HER BİRİNE eklenir.
/// Screenshot'ında gördüğüm gibi her kahvenin boyuta göre farklı fiyatı var.
/// </summary>
public class CoffeeButtonUI : MonoBehaviour
{
    [SerializeField] private CoffeeType coffeeType;
    [SerializeField] private string coffeeName = "Americano";

    [Header("Boyuta Göre Fiyat")]
    [SerializeField] private float priceSmall = 3f;
    [SerializeField] private float priceMedium = 3.5f;
    [SerializeField] private float priceLarge = 4f;

    public CoffeeType CoffeeType => coffeeType;
    public string CoffeeName => coffeeName;

    public float GetPrice(CupSize size)
    {
        switch (size)
        {
            case CupSize.Small: return priceSmall;
            case CupSize.Medium: return priceMedium;
            case CupSize.Large: return priceLarge;
        }
        return 0f;
    }
}