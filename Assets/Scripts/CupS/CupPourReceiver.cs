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
    // ESPRESSO
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


    public void SetEspressoProgress(float progress01)
    {
        if (liquidVisual == null)
            return;

        liquidVisual.SetEspressoProgress(progress01);
    }


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
    // NORMAL SÜT
    // =========================================================

    public bool CanReceiveMilk(MilkFiller source)
    {
        if (source == null)
            return false;

        if (recipe == null)
            return false;

        if (!source.HasMilk)
            return false;

        // Şimdilik espresso olmadan süt dökülmesini engelle.
        if (!recipe.HasEspresso)
            return false;

        // Aynı cup'a ikinci kez normal süt dökülmesin.
        if (recipe.HasMilk)
            return false;

        // Köpüklü süt için ayrı fonksiyon kullanacağız.
        if (source.IsFrothed)
            return false;

        return true;
    }


    public void SetMilkProgress(float progress01)
    {
        if (liquidVisual == null)
            return;

        liquidVisual.SetMilkProgress(progress01);
    }


    public void ReceiveMilk(MilkFiller source)
    {
        if (!CanReceiveMilk(source))
            return;

        recipe.AddMilk();

        if (liquidVisual != null)
        {
            liquidVisual.SetMilkProgress(1f);
        }

        Debug.Log("Normal süt cup'a döküldü.");
    }


    // =========================================================
    // KÖPÜKLÜ SÜT
    // =========================================================

    public bool CanReceiveFrothedMilk(MilkFiller source)
    {
        if (source == null)
            return false;

        if (recipe == null)
            return false;

        if (!source.HasMilk)
            return false;

        if (!source.IsFrothed)
            return false;

        if (!recipe.HasEspresso)
            return false;

        if (recipe.HasFrothedMilk)
            return false;

        return true;
    }


    public void SetFrothedMilkProgress(float progress01)
    {
        if (liquidVisual == null)
            return;

        liquidVisual.SetFrothedMilkProgress(
            progress01
        );
    }


    public void ReceiveFrothedMilk(MilkFiller source)
    {
        if (!CanReceiveFrothedMilk(source))
            return;

        recipe.AddFrothedMilk();

        if (liquidVisual != null)
        {
            liquidVisual.SetFrothedMilkProgress(1f);
        }

        Debug.Log("Köpüklü süt cup'a döküldü.");
    }
}