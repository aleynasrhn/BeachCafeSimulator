using UnityEngine;
using UnityEngine.UI;

public class CoffeeButtonUI : MonoBehaviour
{
    [Header("Kahve")]
    [SerializeField] private CoffeeType coffeeType;
    [SerializeField] private string coffeeName = "Americano";


    [Header("Boyuta Göre Fiyat")]
    [SerializeField] private float priceSmall = 3f;
    [SerializeField] private float priceMedium = 3.5f;
    [SerializeField] private float priceLarge = 4f;


    [Header("Kilit")]
    [SerializeField] private int unlockDay = 1;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private Button button;


    private bool isUnlocked = true;


    public CoffeeType CoffeeType =>
        coffeeType;

    public string CoffeeName =>
        coffeeName;

    public int UnlockDay =>
        unlockDay;

    public bool IsUnlocked =>
        isUnlocked;


    // =========================================================
    // FİYAT
    // =========================================================

    public float GetPrice(CupSize size)
    {
        switch (size)
        {
            case CupSize.Small:
                return priceSmall;

            case CupSize.Medium:
                return priceMedium;

            case CupSize.Large:
                return priceLarge;
        }


        return 0f;
    }


    // =========================================================
    // KİLİT
    // =========================================================

    public void SetUnlocked(
        bool unlocked)
    {
        isUnlocked =
            unlocked;


        if (lockOverlay != null)
        {
            lockOverlay.SetActive(
                !unlocked
            );
        }


        if (button != null)
        {
            button.interactable =
                unlocked;
        }
    }
}