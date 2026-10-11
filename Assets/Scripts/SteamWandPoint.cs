using UnityEngine;

/// <summary>
/// Steam wand'ın ucundaki etkileşim noktası.
///
/// Oyuncu elinde SÜT DOLU pitcher varken buraya bakıp
/// E'ye basılı tutarsa:
/// - Pitcher elinden alınır,
/// - wand'ın altındaki hedef noktaya yumuşakça gider,
/// - SteamButton ısıtma sayacını başlatır.
///
/// Pitcher'ı geri almak için oyuncu pitcher'a E basar (PickupItem).
///
/// Şart: Buhar açık olmalı (düğmeye basılmış), wand altı boş olmalı,
/// süt yanık olmamalı.
/// </summary>
[RequireComponent(typeof(Collider))]
public class SteamWandPoint : MonoBehaviour, IHoldInteractable
{
    [Header("Referanslar")]
    [SerializeField] private SteamButton steamButton;

    [Tooltip("Pitcher'ın wand altında duracağı nokta (boş bir obje).")]
    [SerializeField] private Transform pitcherTargetPoint;


    [Header("Ayarlar")]
    [Tooltip("E'ye ne kadar basılı tutulacak.")]
    [SerializeField] private float holdDuration = 0.8f;

    [Tooltip("Pitcher'ın wand altına gitme süresi.")]
    [SerializeField] private float moveDuration = 0.6f;

    [Tooltip("Pitcher'ın doğal rotasyonuna eklenecek ekstra rotasyon.")]
    [SerializeField] private Vector3 dockedExtraRotationEuler = Vector3.zero;


    // =========================================================
    // HOLD
    // =========================================================

    public float HoldDuration =>
        holdDuration;


    public string GetHoldPrompt()
    {
        if (steamButton == null)
            return "Steam Button atanmamış!";

        if (!steamButton.IsSteamOn)
            return "Önce buharı aç";

        if (steamButton.IsOccupied)
            return "Wand dolu";

        return "E'ye basılı tut (Pitcher'ı wand'a koy)";
    }


    // =========================================================
    // BAŞLAYABİLİR Mİ?
    // =========================================================

    public bool CanStartHold(
        PlayerInteraction player)
    {
        if (player == null)
            return false;

        if (steamButton == null)
            return false;

        if (!steamButton.IsSteamOn)
            return false;

        if (steamButton.IsOccupied)
            return false;

        return TryGetHeldPitcher(
            player,
            out _,
            out _,
            out _
        );
    }


    // =========================================================
    // E BASILIYKEN
    // =========================================================

    public void OnHoldProgress(
        PlayerInteraction player,
        float progress01)
    {
        // Şimdilik ekstra bir şey yok.
    }


    // =========================================================
    // TAMAMLANDI → PITCHER'I WAND'A GÖNDER
    // =========================================================

    public void OnHoldComplete(
        PlayerInteraction player)
    {
        if (!CanStartHold(player))
            return;


        if (!TryGetHeldPitcher(
            player,
            out PickupItem pitcherItem,
            out MilkFiller filler,
            out bool isLeftHand))
        {
            return;
        }


        // -----------------------------------------------------
        // ÖNCE OYUNCUNUN ELİNİ BOŞALT
        // -----------------------------------------------------

        if (isLeftHand)
        {
            player.SetLeftHeldItem(null);
        }
        else
        {
            player.SetHeldItem(null);
        }


        // -----------------------------------------------------
        // PITCHER'I YUMUŞAKÇA WAND'IN ALTINA GÖNDER
        // -----------------------------------------------------

        Vector3 targetPosition =
            pitcherTargetPoint != null
                ? pitcherTargetPoint.position
                : transform.position;


        // NOT: SmoothDockAt, item'ı hareket boyunca kilitler.
        // BeginHeating bu yüzden ondan SONRA çağrılmalı.
        pitcherItem.SmoothDockAt(
            targetPosition,
            dockedExtraRotationEuler,
            moveDuration
        );


        // -----------------------------------------------------
        // SAYACI BAŞLAT (pitcher yerine oturunca saymaya başlar)
        // -----------------------------------------------------

        steamButton.BeginHeating(
            filler,
            pitcherItem
        );
    }


    // =========================================================
    // HOLD İPTAL
    // =========================================================

    public void CancelHold()
    {
        // Yapılacak bir şey yok.
    }


    // =========================================================
    // ELDEKİ PITCHER'I BUL (sağ el, sonra sol el)
    // =========================================================

    private bool TryGetHeldPitcher(
        PlayerInteraction player,
        out PickupItem pitcherItem,
        out MilkFiller filler,
        out bool isLeftHand)
    {
        pitcherItem = null;
        filler = null;
        isLeftHand = false;


        // -----------------------------------------------------
        // SAĞ EL
        // -----------------------------------------------------

        PickupItem held =
            player.GetHeldItem();

        if (IsUsablePitcher(
            held,
            out filler))
        {
            pitcherItem = held;
            isLeftHand = false;

            return true;
        }


        // -----------------------------------------------------
        // SOL EL
        // -----------------------------------------------------

        PickupItem leftHeld =
            player.GetLeftHeldItem();

        if (IsUsablePitcher(
            leftHeld,
            out filler))
        {
            pitcherItem = leftHeld;
            isLeftHand = true;

            return true;
        }


        return false;
    }


    // Süt dolu ve yanmamış bir pitcher mı?
    private bool IsUsablePitcher(
        PickupItem item,
        out MilkFiller filler)
    {
        filler = null;

        if (item == null)
            return false;

        filler =
            item.GetComponent<MilkFiller>();

        if (filler == null)
            return false;

        if (!filler.HasMilk)
            return false;

        // Yanık süt tekrar ısıtılamaz.
        if (filler.IsBurnt)
            return false;

        return true;
    }
}