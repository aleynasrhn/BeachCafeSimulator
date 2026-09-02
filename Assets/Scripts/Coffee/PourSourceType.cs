using UnityEngine;

public enum PourSourceType
{
    Espresso,
    Milk
}


/// <summary>
/// Espresso shot bardağı veya süt kaynağını temsil eder.
///
/// Espresso:
/// - 1 veya 2 shot saklayabilir.
/// - Normal kahve bardağına aktarılabilir.
///
/// Milk:
/// - MilkFiller üzerinden çalışır.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PourSource : MonoBehaviour, IHoldInteractable
{
    [Header("Kaynak Türü")]
    [SerializeField] private PourSourceType sourceType;

    [Header("E Basılı Tutma")]
    [SerializeField] private float holdDuration = 1.5f;

    [Header("Espresso")]
    [Tooltip("Espresso kaynağının PickupItem'ı.")]
    [SerializeField] private PickupItem sourcePickupItem;

    [Header("Milk")]
    [Tooltip("Milk için kullanılacak MilkFiller.")]
    [SerializeField] private MilkFiller sourceMilkFiller;


    // Espresso kaynağındaki shot sayısı.
    private int espressoShotCount = 0;


    public float HoldDuration =>
        holdDuration;


    public int EspressoShotCount =>
        espressoShotCount;


    public bool HasEspresso =>
        sourceType == PourSourceType.Espresso &&
        espressoShotCount > 0;


    // =========================================================
    // ESPRESSO SHOT AYARLA
    // =========================================================

    public void SetEspressoShots(int shotCount)
    {
        if (sourceType != PourSourceType.Espresso)
            return;


        espressoShotCount =
            Mathf.Max(
                0,
                shotCount
            );


        Debug.Log(
            $"Espresso kaynağına " +
            $"{espressoShotCount} shot yüklendi."
        );
    }


    // =========================================================
    // ESPRESSO TÜMÜNÜ TÜKET
    // =========================================================

    public void ConsumeEspressoShots()
    {
        if (sourceType != PourSourceType.Espresso)
            return;


        espressoShotCount = 0;


        if (sourcePickupItem != null)
        {
            sourcePickupItem.EmptyEspresso();
        }


        Debug.Log(
            "Espresso kaynağı tamamen boşaldı."
        );
    }


    // =========================================================
    // PROMPT
    // =========================================================

    public string GetHoldPrompt()
    {
        if (sourceType ==
            PourSourceType.Espresso)
        {
            return
                "E'ye basılı tut (Espresso Dök)";
        }


        return
            "E'ye basılı tut (Süt Dök)";
    }


    // =========================================================
    // BAŞLAYABİLİR Mİ?
    // =========================================================

    public bool CanStartHold(
        PlayerInteraction player)
    {
        if (player == null)
            return false;


        PickupItem held =
            player.GetHeldItem();


        if (held == null)
            return false;


        DrinkRecipe recipe =
            held.GetComponent<DrinkRecipe>();


        if (recipe == null)
            return false;


        // -----------------------------------------------------
        // ESPRESSO
        // -----------------------------------------------------

        if (sourceType ==
            PourSourceType.Espresso)
        {
            CupPourReceiver receiver =
                held.GetComponent<CupPourReceiver>();


            if (receiver == null)
                return false;


            return
                HasEspresso &&
                receiver.CanReceiveEspresso(this);
        }


        // -----------------------------------------------------
        // MILK
        // -----------------------------------------------------

        return
            sourceMilkFiller != null &&
            sourceMilkFiller.HasMilk;
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


        PickupItem held =
            player.GetHeldItem();


        if (held == null)
            return;


        CupPourReceiver receiver =
            held.GetComponent<CupPourReceiver>();


        if (receiver == null)
            return;


        // -----------------------------------------------------
        // ESPRESSO
        // -----------------------------------------------------

        if (sourceType ==
            PourSourceType.Espresso)
        {
            receiver.SetEspressoProgress(
                progress01
            );

            return;
        }


        // -----------------------------------------------------
        // MILK
        // -----------------------------------------------------

        if (sourceMilkFiller != null)
        {
            if (sourceMilkFiller.IsFrothed)
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


        PickupItem held =
            player.GetHeldItem();


        if (held == null)
            return;


        CupPourReceiver receiver =
            held.GetComponent<CupPourReceiver>();


        if (receiver == null)
            return;


        // -----------------------------------------------------
        // ESPRESSO
        // -----------------------------------------------------

        if (sourceType ==
            PourSourceType.Espresso)
        {
            receiver.ReceiveEspresso(
                this
            );

            return;
        }


        // -----------------------------------------------------
        // MILK
        // -----------------------------------------------------

        if (sourceMilkFiller == null)
            return;


        if (sourceMilkFiller.IsFrothed)
        {
            receiver.ReceiveFrothedMilk(
                sourceMilkFiller
            );
        }
        else
        {
            receiver.ReceiveMilk(
                sourceMilkFiller
            );
        }
    }


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (sourcePickupItem == null)
        {
            sourcePickupItem =
                GetComponent<PickupItem>();
        }
    }
}