using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(CupPourReceiver))]
public class CupPourInteraction : MonoBehaviour, IHoldInteractable
{
    [Header("Ayarlar")]
    [SerializeField] private float holdDuration = 1.5f;


    [Header("UI")]
    [SerializeField]
    private string pourPrompt =
        "E'ye basılı tut";


    private CupPourReceiver receiver;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        receiver =
            GetComponent<CupPourReceiver>();
    }


    public float HoldDuration =>
        holdDuration;


    // =========================================================
    // PROMPT
    // =========================================================

    public string GetHoldPrompt()
    {
        return pourPrompt;
    }


    // =========================================================
    // BAŞLAYABİLİR Mİ?
    // =========================================================

    public bool CanStartHold(
        PlayerInteraction player)
    {
        if (player == null)
            return false;


        if (receiver == null)
            return false;


        // =====================================================
        // SAĞ EL
        // =====================================================

        PickupItem held =
            player.GetHeldItem();


        if (held != null)
        {
            // -------------------------------------------------
            // ESPRESSO
            // -------------------------------------------------

            PourSource espressoSource =
                held.GetComponent<PourSource>();


            if (espressoSource != null &&
                espressoSource.HasEspresso)
            {
                return receiver.CanReceiveEspresso(
                    espressoSource
                );
            }


            // -------------------------------------------------
            // MILK SOURCE
            // -------------------------------------------------

            MilkSource milkSource =
                held.GetComponent<MilkSource>();


            if (milkSource != null)
            {
                return receiver.CanReceiveMilkSource(
                    milkSource
                );
            }


            // -------------------------------------------------
            // MILK PITCHER
            // -------------------------------------------------

            MilkFiller milkFiller =
                held.GetComponent<MilkFiller>();


            if (milkFiller != null)
            {
                if (milkFiller.IsFrothed)
                {
                    return receiver.CanReceiveFrothedMilk(
                        milkFiller
                    );
                }


                return receiver.CanReceiveMilk(
                    milkFiller
                );
            }


            // -------------------------------------------------
            // KETTLE
            // -------------------------------------------------

            KettleWaterState kettleWater =
                held.GetComponent<KettleWaterState>();


            if (kettleWater != null)
            {
                return receiver.CanReceiveHotWater(
                    kettleWater
                );
            }
        }


        // =====================================================
        // SOL EL
        // =====================================================

        PickupItem leftHeld =
            player.GetLeftHeldItem();


        if (leftHeld != null)
        {
            // -------------------------------------------------
            // ESPRESSO
            // -------------------------------------------------

            PourSource espressoSource =
                leftHeld.GetComponent<PourSource>();


            if (espressoSource != null &&
                espressoSource.HasEspresso)
            {
                return receiver.CanReceiveEspresso(
                    espressoSource
                );
            }


            // -------------------------------------------------
            // MILK SOURCE
            // -------------------------------------------------

            MilkSource milkSource =
                leftHeld.GetComponent<MilkSource>();


            if (milkSource != null)
            {
                return receiver.CanReceiveMilkSource(
                    milkSource
                );
            }


            // -------------------------------------------------
            // MILK PITCHER
            // -------------------------------------------------

            MilkFiller milkFiller =
                leftHeld.GetComponent<MilkFiller>();


            if (milkFiller != null)
            {
                if (milkFiller.IsFrothed)
                {
                    return receiver.CanReceiveFrothedMilk(
                        milkFiller
                    );
                }


                return receiver.CanReceiveMilk(
                    milkFiller
                );
            }


            // -------------------------------------------------
            // KETTLE
            // -------------------------------------------------

            KettleWaterState kettleWater =
                leftHeld.GetComponent<KettleWaterState>();


            if (kettleWater != null)
            {
                return receiver.CanReceiveHotWater(
                    kettleWater
                );
            }
        }


        return false;
    }


    // =========================================================
    // E BASILIYKEN
    // =========================================================

    public void OnHoldProgress(
        PlayerInteraction player,
        float progress01)
    {
        if (player == null)
            return;


        if (receiver == null)
            return;


        // =====================================================
        // SAĞ EL
        // =====================================================

        PickupItem held =
            player.GetHeldItem();


        if (held != null)
        {
            // -------------------------------------------------
            // ESPRESSO
            // -------------------------------------------------

            PourSource espressoSource =
                held.GetComponent<PourSource>();


            if (espressoSource != null &&
                espressoSource.HasEspresso)
            {
                receiver.SetEspressoProgress(
                    progress01
                );

                return;
            }


            // -------------------------------------------------
            // MILK SOURCE
            // -------------------------------------------------

            MilkSource milkSource =
                held.GetComponent<MilkSource>();


            if (milkSource != null)
            {
                receiver.SetMilkSourceProgress(
                    progress01
                );

                return;
            }


            // -------------------------------------------------
            // MILK PITCHER
            // -------------------------------------------------

            MilkFiller milkFiller =
                held.GetComponent<MilkFiller>();


            if (milkFiller != null)
            {
                if (milkFiller.IsFrothed)
                {
                    receiver.SetFrothedMilkProgress(
                        progress01
                    );
                }
                else
                {
                    receiver.SetMilkProgress(
                        progress01
                    );
                }

                return;
            }


            // -------------------------------------------------
            // KETTLE
            // -------------------------------------------------

            KettleWaterState kettleWater =
                held.GetComponent<KettleWaterState>();


            if (kettleWater != null)
            {
                receiver.SetWaterProgress(
                    progress01
                );

                return;
            }
        }


        // =====================================================
        // SOL EL
        // =====================================================

        PickupItem leftHeld =
            player.GetLeftHeldItem();


        if (leftHeld != null)
        {
            PourSource espressoSource =
                leftHeld.GetComponent<PourSource>();


            if (espressoSource != null &&
                espressoSource.HasEspresso)
            {
                receiver.SetEspressoProgress(
                    progress01
                );

                return;
            }


            MilkSource milkSource =
                leftHeld.GetComponent<MilkSource>();


            if (milkSource != null)
            {
                receiver.SetMilkSourceProgress(
                    progress01
                );

                return;
            }


            MilkFiller milkFiller =
                leftHeld.GetComponent<MilkFiller>();


            if (milkFiller != null)
            {
                if (milkFiller.IsFrothed)
                {
                    receiver.SetFrothedMilkProgress(
                        progress01
                    );
                }
                else
                {
                    receiver.SetMilkProgress(
                        progress01
                    );
                }

                return;
            }


            KettleWaterState kettleWater =
                leftHeld.GetComponent<KettleWaterState>();


            if (kettleWater != null)
            {
                receiver.SetWaterProgress(
                    progress01
                );

                return;
            }
        }
    }


    // =========================================================
    // DÖKME TAMAMLANDI
    // =========================================================

    public void OnHoldComplete(
        PlayerInteraction player)
    {
        if (player == null)
            return;


        if (receiver == null)
            return;


        // =====================================================
        // SAĞ EL
        // =====================================================

        PickupItem held =
            player.GetHeldItem();


        if (held != null)
        {
            // -------------------------------------------------
            // ESPRESSO
            // -------------------------------------------------

            PourSource espressoSource =
                held.GetComponent<PourSource>();


            if (espressoSource != null &&
                espressoSource.HasEspresso)
            {
                receiver.ReceiveEspresso(
                    espressoSource
                );

                return;
            }


            // -------------------------------------------------
            // MILK SOURCE
            // -------------------------------------------------

            MilkSource milkSource =
                held.GetComponent<MilkSource>();


            if (milkSource != null)
            {
                receiver.ReceiveMilkSource(
                    milkSource
                );

                return;
            }


            // -------------------------------------------------
            // MILK PITCHER
            // -------------------------------------------------

            MilkFiller milkFiller =
                held.GetComponent<MilkFiller>();


            if (milkFiller != null)
            {
                if (milkFiller.IsFrothed)
                {
                    receiver.ReceiveFrothedMilk(
                        milkFiller
                    );
                }
                else
                {
                    receiver.ReceiveMilk(
                        milkFiller
                    );
                }

                return;
            }


            // -------------------------------------------------
            // KETTLE
            // -------------------------------------------------

            KettleWaterState kettleWater =
                held.GetComponent<KettleWaterState>();


            if (kettleWater != null)
            {
                receiver.ReceiveHotWater(
                    kettleWater
                );

                return;
            }
        }


        // =====================================================
        // SOL EL
        // =====================================================

        PickupItem leftHeld =
            player.GetLeftHeldItem();


        if (leftHeld != null)
        {
            // -------------------------------------------------
            // ESPRESSO
            // -------------------------------------------------

            PourSource espressoSource =
                leftHeld.GetComponent<PourSource>();


            if (espressoSource != null &&
                espressoSource.HasEspresso)
            {
                receiver.ReceiveEspresso(
                    espressoSource
                );

                return;
            }


            // -------------------------------------------------
            // MILK SOURCE
            // -------------------------------------------------

            MilkSource milkSource =
                leftHeld.GetComponent<MilkSource>();


            if (milkSource != null)
            {
                receiver.ReceiveMilkSource(
                    milkSource
                );

                return;
            }


            // -------------------------------------------------
            // MILK PITCHER
            // -------------------------------------------------

            MilkFiller milkFiller =
                leftHeld.GetComponent<MilkFiller>();


            if (milkFiller != null)
            {
                if (milkFiller.IsFrothed)
                {
                    receiver.ReceiveFrothedMilk(
                        milkFiller
                    );
                }
                else
                {
                    receiver.ReceiveMilk(
                        milkFiller
                    );
                }

                return;
            }


            // -------------------------------------------------
            // KETTLE
            // -------------------------------------------------

            KettleWaterState kettleWater =
                leftHeld.GetComponent<KettleWaterState>();


            if (kettleWater != null)
            {
                receiver.ReceiveHotWater(
                    kettleWater
                );

                return;
            }
        }
    }
}