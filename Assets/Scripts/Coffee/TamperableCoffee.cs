using UnityEngine;
using System.Collections;

/// <summary>
/// Portafilter objesine eklenir.
///
/// Akış:
/// 1) Oyuncu sol elinde tamper varken E'yi basılı tutar.
/// 2) Tamper portafilterin içine doğru hareket eder.
/// 3) Basılı tutma tamamlanınca portafilter kilitlenir.
/// 4) Tamper belirlenen süre boyunca yerinde sabit kalır.
/// 5) Süre bitince kahve tamp edilir.
/// 6) Tamper eski yerine döner ve portafilterin kilidi açılır.
/// </summary>
[RequireComponent(typeof(PickupItem))]
public class TamperableCoffee : MonoBehaviour, IHoldInteractable
{
    [Header("Basılı tutma")]
    [SerializeField] private float holdDuration = 3f;

    [Header("Tamper oturduktan sonra bekleme")]
    [SerializeField] private float settleDuration = 2.5f;

    [SerializeField] private string tamperItemName = "tamper";

    [Tooltip("Tamper portafilterin içine oturduğunda yükseklik ofseti.")]
    [SerializeField] private float seatedHeightOffset = 0.02f;


    private PickupItem pickupItem;

    private bool isProcessing = false;


    private void Awake()
    {
        pickupItem = GetComponent<PickupItem>();
    }


    public float HoldDuration =>
        holdDuration;


    public string GetHoldPrompt()
    {
        return "E'ye basılı tut (Tampla)";
    }


    public bool CanStartHold(PlayerInteraction player)
    {
        if (isProcessing)
            return false;


        PickupItem leftHeld =
            player.GetLeftHeldItem();


        if (leftHeld == null)
            return false;


        if (leftHeld.ItemName != tamperItemName)
            return false;


        if (!pickupItem.HasGroundCoffee)
            return false;


        if (pickupItem.IsTamped)
            return false;


        return true;
    }


    public void OnHoldProgress(
        PlayerInteraction player,
        float progress01)
    {
        PickupItem tamper =
            player.GetLeftHeldItem();


        if (tamper == null)
            return;


        Vector3 seatedPos =
            transform.position +
            Vector3.up *
            seatedHeightOffset;


        Vector3 startPos =
            player.LeftHoldPoint.position;


        Vector3 animatedPos =
            Vector3.Lerp(
                startPos,
                seatedPos,
                progress01
            );


        tamper.OverrideHeldPosition(
            animatedPos,
            transform.rotation
        );
    }


    public void OnHoldComplete(
        PlayerInteraction player)
    {
        PickupItem tamper =
            player.GetLeftHeldItem();


        if (tamper == null)
            return;


        StartCoroutine(
            SettleAndTampRoutine(
                player,
                tamper
            )
        );
    }


    private IEnumerator SettleAndTampRoutine(
        PlayerInteraction player,
        PickupItem tamper)
    {
        isProcessing = true;


        // =====================================================
        // PORTAFILTERİ KİLİTLE
        // =====================================================

        pickupItem.SetInteractionLocked(true);


        Vector3 seatedPos =
            transform.position +
            Vector3.up *
            seatedHeightOffset;


        Quaternion seatedRot =
            transform.rotation;


        float t = 0f;


        // =====================================================
        // TAMPER OTURMUŞ DURUMDA BEKLER
        // =====================================================

        while (t < settleDuration)
        {
            tamper.OverrideHeldPosition(
                seatedPos,
                seatedRot
            );


            t += Time.deltaTime;

            yield return null;
        }


        // =====================================================
        // KAHVEYİ TAMPLE
        // =====================================================

        pickupItem.TampCoffee();


        // =====================================================
        // TAMPERİ GERİ GÖNDER
        // =====================================================

        tamper.ClearHeldPositionOverride();

        tamper.ReturnToOriginalPosition();

        player.SetLeftHeldItem(null);


        // =====================================================
        // PORTAFILTER KİLİDİNİ AÇ
        // =====================================================

        pickupItem.SetInteractionLocked(false);


        isProcessing = false;
    }
}