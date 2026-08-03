using UnityEngine;
using System.Collections;

/// <summary>
/// Portafilter (portafilter.001) objesine, PickupItem'ın YANINA eklenir.
///
/// Akış:
/// 1) Oyuncu SOL elinde tamper varken (portafilter dolu, henüz tamperlenmemiş) buraya bakıp
///    E'yi basılı tutar -> tamper görsel olarak (holdDuration süresince) kayarak portafilterin
///    içine oturur (OnHoldProgress her frame çağrılır, tamperi lerp'ler).
/// 2) Basılı tutma tamamlanınca (OnHoldComplete) -> tamper artık YERİNDE SABİT kalır
///    (settleDuration kadar - oyuncu E'yi bıraksa, kamerayı çevirse bile değişmez, çünkü
///    coroutine her frame pozisyonu kendi zorluyor).
/// 3) settleDuration bitince kahve tamperlenir, tamper otomatik sol eldeki ORİJİNAL
///    yerine geri döner, oyuncunun sol eli boşalır.
/// </summary>
[RequireComponent(typeof(PickupItem))]
public class TamperableCoffee : MonoBehaviour, IHoldInteractable
{
    [Header("Basılı tutma (tamper kayıp oturana kadar)")]
    [SerializeField] private float holdDuration = 3f;

    [Header("Oturduktan sonra bekleme (tamperlenmeden önce)")]
    [SerializeField] private float settleDuration = 2.5f;

    [SerializeField] private string tamperItemName = "tamper";
    [Tooltip("Tamper portafilterin içine oturduğunda, portafilterin pivotuna göre ne kadar yukarıda duracağı")]
    [SerializeField] private float seatedHeightOffset = 0.02f;

    private PickupItem pickupItem;
    private bool isProcessing = false; // basılı tutma bitti, settle/tamp aşamasındayken true

    private void Awake()
    {
        pickupItem = GetComponent<PickupItem>();
    }

    public float HoldDuration => holdDuration;

    public string GetHoldPrompt()
    {
        return "E'ye basılı tut (Tampla)";
    }

    public bool CanStartHold(PlayerInteraction player)
    {
        if (isProcessing) return false; // zaten tamperleme sürecinde, tekrar başlatılamaz

        PickupItem leftHeld = player.GetLeftHeldItem(); // tamper SOL elde olmalı
        if (leftHeld == null) return false;
        if (leftHeld.ItemName != tamperItemName) return false;
        if (!pickupItem.HasGroundCoffee) return false;
        if (pickupItem.IsTamped) return false;
        return true;
    }

    public void OnHoldProgress(PlayerInteraction player, float progress01)
    {
        PickupItem tamper = player.GetLeftHeldItem();
        if (tamper == null) return;

        // Basit, doğrusal bir "kayma" - tamper elden portafilterin içine doğru ilerler
        Vector3 seatedPos = transform.position + Vector3.up * seatedHeightOffset;
        Vector3 startPos = player.LeftHoldPoint.position;
        Vector3 animatedPos = Vector3.Lerp(startPos, seatedPos, progress01);

        tamper.OverrideHeldPosition(animatedPos, transform.rotation);
    }

    public void OnHoldComplete(PlayerInteraction player)
    {
        PickupItem tamper = player.GetLeftHeldItem();
        if (tamper == null) return;

        StartCoroutine(SettleAndTampRoutine(player, tamper));
    }

    private IEnumerator SettleAndTampRoutine(PlayerInteraction player, PickupItem tamper)
    {
        isProcessing = true;

        Vector3 seatedPos = transform.position + Vector3.up * seatedHeightOffset;
        Quaternion seatedRot = transform.rotation;

        float t = 0f;
        while (t < settleDuration)
        {
            // Her frame yeniden uyguluyoruz ki oyuncu E'yi bıraksa/kamerayı çevirse bile
            // tamper portafilterin üstünde sabit kalsın.
            tamper.OverrideHeldPosition(seatedPos, seatedRot);
            t += Time.deltaTime;
            yield return null;
        }

        pickupItem.TampCoffee();

        tamper.ClearHeldPositionOverride();
        tamper.ReturnToOriginalPosition(); // sol eldeki yerine otomatik döner
        player.SetLeftHeldItem(null);       // sol el boşalır

        isProcessing = false;
    }
}