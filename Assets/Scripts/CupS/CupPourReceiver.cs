using UnityEngine;

public class CupPourReceiver : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private PickupItem cup;
    [SerializeField] private DrinkRecipe recipe;
    [SerializeField] private CupLiquidVisual liquidVisual;


    private void Awake()
    {
        if (cup == null)
        {
            cup = GetComponent<PickupItem>();
        }

        if (recipe == null)
        {
            recipe = GetComponent<DrinkRecipe>();
        }

        if (liquidVisual == null)
        {
            liquidVisual =
                GetComponentInChildren<CupLiquidVisual>();
        }
    }


    // =========================================================
    // ESPRESSO KONTROL
    // =========================================================

    public bool CanReceiveEspresso(PickupItem source)
    {
        if (source == null)
            return false;

        if (cup == null)
            return false;

        if (recipe == null)
            return false;

        if (!source.HasEspresso)
            return false;

        if (recipe.HasEspresso)
            return false;

        return true;
    }


    // =========================================================
    // ESPRESSO DÖKÜLÜRKEN
    // =========================================================

    public void SetEspressoProgress(float progress01)
    {
        if (liquidVisual == null)
            return;

        liquidVisual.SetEspressoProgress(
            progress01
        );
    }


    // =========================================================
    // ESPRESSO TAMAMLANDI
    // =========================================================

    public void ReceiveEspresso(PickupItem source)
    {
        if (!CanReceiveEspresso(source))
            return;

        recipe.AddEspresso();

        cup.FillWithEspresso();

        source.EmptyEspresso();

        if (liquidVisual != null)
        {
            liquidVisual.SetEspressoProgress(1f);
        }

        Debug.Log("Espresso cup'a döküldü.");
    }


    // =========================================================
    // SÜT KONTROL
    // =========================================================

    public bool CanReceiveMilk(MilkFiller source)
    {
        if (source == null)
            return false;

        if (recipe == null)
            return false;

        if (!source.HasMilk)
            return false;

        return true;
    }


    // =========================================================
    // SÜT DÖKÜLÜRKEN
    // =========================================================

    public void SetMilkProgress(
        float progress01,
        bool isFrothed)
    {
        if (liquidVisual == null)
            return;

        if (isFrothed)
        {
            liquidVisual.SetFrothedMilkProgress(
                progress01
            );
        }
        else
        {
            liquidVisual.SetMilkProgress(
                progress01
            );
        }
    }


    // =========================================================
    // SÜT TAMAMLANDI
    // =========================================================

    public void ReceiveMilk(MilkFiller source)
    {
        if (!CanReceiveMilk(source))
            return;

        if (source.IsFrothed)
        {
            recipe.AddFrothedMilk();

            liquidVisual?.SetFrothedMilkProgress(1f);
        }
        else
        {
            recipe.AddMilk();

            liquidVisual?.SetMilkProgress(1f);
        }

        Debug.Log("Süt cup'a döküldü.");
    }
}