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


    public void SetEspressoProgress(
        float progress01)
    {
        if (liquidVisual == null)
            return;

        liquidVisual.SetEspressoProgress(
            progress01
        );
    }


    public void ReceiveEspresso(
        PickupItem source)
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

        Debug.Log(
            "Espresso cup'a döküldü."
        );
    }


    // =========================================================
    // MILK SOURCE
    // SÜT KUTUSU
    // =========================================================

    public bool CanReceiveMilkSource(
        MilkSource source)
    {
        if (source == null)
            return false;

        if (recipe == null)
            return false;

        // Önce espresso bulunmalı.
        if (!recipe.HasEspresso)
            return false;

        // Milk kutusu süt vermeli.
        if (!source.HasMilk)
            return false;

        // Aynı cup ikinci kez normal süt almasın.
        if (recipe.HasMilk)
            return false;

        // Köpüklü süt ile çakışmasın.
        if (recipe.HasFrothedMilk)
            return false;

        return true;
    }


    public void SetMilkSourceProgress(
        float progress01)
    {
        if (liquidVisual == null)
            return;

        liquidVisual.SetMilkProgress(
            progress01
        );
    }


    public void ReceiveMilkSource(
        MilkSource source)
    {
        if (!CanReceiveMilkSource(source))
            return;

        recipe.AddMilk();

        if (liquidVisual != null)
        {
            liquidVisual.SetMilkProgress(1f);
        }

        // MilkSource tüketilmiyor.
        // Aynı kutudan başka cup'lara da süt verilebilir.

        Debug.Log(
            "Milk kutusundan süt cup'a döküldü."
        );
    }


    // =========================================================
    // MILK PITCHER
    // NORMAL SÜT
    // =========================================================

    public bool CanReceiveMilk(
        MilkFiller source)
    {
        if (source == null)
            return false;

        if (recipe == null)
            return false;

        if (!source.HasMilk)
            return false;

        // Köpüklü süt ayrı işlem.
        if (source.IsFrothed)
            return false;

        // Önce espresso.
        if (!recipe.HasEspresso)
            return false;

        // Cup daha önce normal süt aldıysa tekrar alma.
        if (recipe.HasMilk)
            return false;

        // Cappuccino sütüyle çakışmasın.
        if (recipe.HasFrothedMilk)
            return false;

        return true;
    }


    public void SetMilkProgress(
        float progress01)
    {
        if (liquidVisual == null)
            return;

        liquidVisual.SetMilkProgress(
            progress01
        );
    }


    public void ReceiveMilk(
        MilkFiller source)
    {
        if (!CanReceiveMilk(source))
            return;

        recipe.AddMilk();

        if (liquidVisual != null)
        {
            liquidVisual.SetMilkProgress(1f);
        }

        // Pitcher tamamen boşalır.
        source.ConsumeAllMilk();

        Debug.Log(
            "Normal süt pitcher'dan cup'a döküldü."
        );
    }


    // =========================================================
    // KÖPÜKLÜ SÜT
    // MILK PITCHER
    // =========================================================

    public bool CanReceiveFrothedMilk(
        MilkFiller source)
    {
        if (source == null)
            return false;

        if (recipe == null)
            return false;

        if (!source.HasMilk)
            return false;

        if (!source.IsFrothed)
            return false;

        // Önce espresso.
        if (!recipe.HasEspresso)
            return false;

        // Aynı cup ikinci kez köpüklü süt almasın.
        if (recipe.HasFrothedMilk)
            return false;

        // Latte sütüyle çakışmasın.
        if (recipe.HasMilk)
            return false;

        return true;
    }


    public void SetFrothedMilkProgress(
        float progress01)
    {
        if (liquidVisual == null)
            return;

        liquidVisual.SetFrothedMilkProgress(
            progress01
        );
    }


    public void ReceiveFrothedMilk(
        MilkFiller source)
    {
        if (!CanReceiveFrothedMilk(source))
            return;

        recipe.AddFrothedMilk();

        if (liquidVisual != null)
        {
            liquidVisual.SetFrothedMilkProgress(1f);
        }

        // Köpüklü pitcher da tamamen boşalır.
        source.ConsumeAllMilk();

        Debug.Log(
            "Köpüklü süt cup'a döküldü."
        );
    }
}