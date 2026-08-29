using UnityEngine;

/// <summary>
/// Cup'a espresso, normal süt veya köpüklü süt dökme işlemini yönetir.
///
/// Kaynaklar:
/// - Espresso Shot
/// - Milk kutusu
/// - Normal süt içeren Milk Pitcher
/// - Köpürtülmüş Milk Pitcher
/// </summary>
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
    // DÖKME BAŞLATABİLİR Mİ?
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

            if (held.HasEspresso)
            {
                return receiver.CanReceiveEspresso(
                    held
                );
            }


            // -------------------------------------------------
            // MILK KUTUSU
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
                // Köpürtülmüş süt
                if (milkFiller.IsFrothed)
                {
                    return receiver.CanReceiveFrothedMilk(
                        milkFiller
                    );
                }

                // Normal süt
                return receiver.CanReceiveMilk(
                    milkFiller
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

            if (leftHeld.HasEspresso)
            {
                return receiver.CanReceiveEspresso(
                    leftHeld
                );
            }


            // -------------------------------------------------
            // MILK KUTUSU
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

            if (held.HasEspresso)
            {
                receiver.SetEspressoProgress(
                    progress01
                );

                return;
            }


            // -------------------------------------------------
            // MILK KUTUSU
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

            if (leftHeld.HasEspresso)
            {
                receiver.SetEspressoProgress(
                    progress01
                );

                return;
            }


            // -------------------------------------------------
            // MILK KUTUSU
            // -------------------------------------------------

            MilkSource milkSource =
                leftHeld.GetComponent<MilkSource>();

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

            if (held.HasEspresso)
            {
                receiver.ReceiveEspresso(
                    held
                );

                return;
            }


            // -------------------------------------------------
            // MILK KUTUSU
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

            if (leftHeld.HasEspresso)
            {
                receiver.ReceiveEspresso(
                    leftHeld
                );

                return;
            }


            // -------------------------------------------------
            // MILK KUTUSU
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
        }
    }
}