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
            cup =
                GetComponent<PickupItem>();
        }


        if (recipe == null)
        {
            recipe =
                GetComponent<DrinkRecipe>();
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

    public bool CanReceiveEspresso(
        PourSource source)
    {
        if (source == null)
            return false;


        if (cup == null)
            return false;


        if (recipe == null)
            return false;


        if (!source.HasEspresso)
            return false;


        // Cup'ta espresso varsa
        // ikinci kez espresso dökülmesin.
        if (recipe.HasEspresso)
            return false;


        // En az 1 shot olmalı.
        if (source.EspressoShotCount <= 0)
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
        PourSource source)
    {
        if (!CanReceiveEspresso(source))
            return;


        int shotCount =
            source.EspressoShotCount;


        // Kaynaktaki shot sayısı kadar
        // espresso bardağa ekle.
        for (int i = 0;
             i < shotCount;
             i++)
        {
            recipe.AddEspresso();
        }


        cup.FillWithEspresso();


        if (liquidVisual != null)
        {
            liquidVisual.SetEspressoProgress(
                1f
            );
        }


        // Espresso kaynağını boşalt.
        source.ConsumeEspressoShots();


        Debug.Log(
            $"Espresso cup'a döküldü. " +
            $"Shot sayısı: {shotCount}"
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


        if (!recipe.HasEspresso)
            return false;


        if (!source.HasMilk)
            return false;


        if (recipe.HasMilk)
            return false;


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
            liquidVisual.SetMilkProgress(
                1f
            );
        }


        Debug.Log(
            "Milk kutusundan süt cup'a döküldü."
        );
    }


    // =========================================================
    // MILK PITCHER
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


        if (source.IsFrothed)
            return false;


        if (!recipe.HasEspresso)
            return false;


        if (recipe.HasMilk)
            return false;


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
            liquidVisual.SetMilkProgress(
                1f
            );
        }


        source.ConsumeAllMilk();


        Debug.Log(
            "Normal süt pitcher'dan cup'a döküldü."
        );
    }


    // =========================================================
    // KÖPÜKLÜ SÜT
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


        if (!recipe.HasEspresso)
            return false;


        if (recipe.HasFrothedMilk)
            return false;


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
            liquidVisual.SetFrothedMilkProgress(
                1f
            );
        }


        source.ConsumeAllMilk();


        Debug.Log(
            "Köpüklü süt cup'a döküldü."
        );
    }


    // =========================================================
    // SICAK SU
    // =========================================================

    public bool CanReceiveHotWater(
        KettleWaterState source)
    {
        if (source == null)
            return false;


        if (recipe == null)
            return false;


        if (!recipe.HasEspresso)
            return false;


        if (!source.HasWater)
            return false;


        if (!source.IsHot)
            return false;


        if (recipe.HasHotWater)
            return false;


        if (recipe.HasMilk)
            return false;


        if (recipe.HasFrothedMilk)
            return false;


        return true;
    }


    public void SetWaterProgress(
        float progress01)
    {
        if (liquidVisual == null)
            return;


        liquidVisual.SetWaterProgress(
            progress01
        );
    }


    public void ReceiveHotWater(
        KettleWaterState source)
    {
        if (!CanReceiveHotWater(source))
            return;


        recipe.AddHotWater();


        if (liquidVisual != null)
        {
            liquidVisual.SetWaterProgress(
                1f
            );
        }


        bool consumed =
            source.ConsumeHotWater();


        if (!consumed)
        {
            Debug.LogWarning(
                "Sıcak su tüketilemedi."
            );

            return;
        }


        Debug.Log(
            "Sıcak su cup'a döküldü. " +
            "Americano hazır."
        );
    }
}