using UnityEngine;

/// <summary>
/// Cup'a espresso veya süt dökme işlemini yönetir.
/// Oyuncu elinde kaynak varken cup'a bakıp E'yi basılı tuttuğunda
/// içerik cup'a aktarılır.
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
            // Espresso shot
            if (held.HasEspresso)
            {
                return receiver.CanReceiveEspresso(
                    held
                );
            }


            // Milk pitcher
            MilkFiller milkFiller =
                held.GetComponent<MilkFiller>();

            if (milkFiller != null)
            {
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
            // Espresso shot
            if (leftHeld.HasEspresso)
            {
                return receiver.CanReceiveEspresso(
                    leftHeld
                );
            }


            // Milk pitcher
            MilkFiller milkFiller =
                leftHeld.GetComponent<MilkFiller>();

            if (milkFiller != null)
            {
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
            // Espresso
            if (held.HasEspresso)
            {
                receiver.SetEspressoProgress(
                    progress01
                );

                return;
            }


            // Süt
            MilkFiller milkFiller =
                held.GetComponent<MilkFiller>();

            if (milkFiller != null)
            {
                receiver.SetMilkProgress(
                    progress01,
                    milkFiller.IsFrothed
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
            // Espresso
            if (leftHeld.HasEspresso)
            {
                receiver.SetEspressoProgress(
                    progress01
                );

                return;
            }


            // Süt
            MilkFiller milkFiller =
                leftHeld.GetComponent<MilkFiller>();

            if (milkFiller != null)
            {
                receiver.SetMilkProgress(
                    progress01,
                    milkFiller.IsFrothed
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
            // Espresso
            if (held.HasEspresso)
            {
                receiver.ReceiveEspresso(
                    held
                );

                return;
            }


            // Süt
            MilkFiller milkFiller =
                held.GetComponent<MilkFiller>();

            if (milkFiller != null)
            {
                receiver.ReceiveMilk(
                    milkFiller
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
            // Espresso
            if (leftHeld.HasEspresso)
            {
                receiver.ReceiveEspresso(
                    leftHeld
                );

                return;
            }


            // Süt
            MilkFiller milkFiller =
                leftHeld.GetComponent<MilkFiller>();

            if (milkFiller != null)
            {
                receiver.ReceiveMilk(
                    milkFiller
                );

                return;
            }
        }
    }
}