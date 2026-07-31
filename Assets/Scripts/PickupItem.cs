using UnityEngine;

/// <summary>
/// Portafilter, espresso cup, milk pitcher, tamper gibi elle tutulabilen
/// her objeye bu scripti eklersin. Rigidbody + Collider zorunlu (RequireComponent ile garanti).
///
/// ONEMLI: Bu script hicbir zaman transform.SetParent() cagirmaz. Sebebi: Player/Camera/HoldPoint
/// zincirinde esit olmayan (non-uniform) scale + rotasyon oldugunda, SetParent her cagrildiginda
/// Unity'nin dunya boyutunu korumak icin yaptigi hesap hatali oluyor ve obje her pickup/drop
/// dongusunde biraz daha carpitiyordu (scale drift). Bunun yerine obje hep ayni parent'ta kalir,
/// sadece world-space pozisyon/rotasyon elle guncellenir - scale'e HICBIR ZAMAN dokunulmaz.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("Ayarlar")]
    [SerializeField] private string itemName = "Item";
    [SerializeField] private float holdSmoothSpeed = 15f; // elde tutarken ne kadar yumuşak takip etsin
    [SerializeField] private float surfaceYOffset = 0f; // objenin pivot noktasından tabanına kadar mesafe

    [Header("Elde Tutma Ayarı (pivot farklıysa buradan düzelt)")]
    [Tooltip("HoldPoint'e göre ek pozisyon offseti - obje kameraya çok yakın/gömülü görünüyorsa buradan ileri/geri/yana kaydır")]
    [SerializeField] private Vector3 holdPositionOffset = Vector3.zero;
    [Tooltip("HoldPoint'in rotasyonuna eklenecek ek açı - obje elde yamuk/yan duruyorsa buradan düzelt")]
    [SerializeField] private Vector3 holdRotationOffsetEuler = Vector3.zero;

    private Rigidbody rb;
    private Collider col;
    private Transform holdPoint;
    private bool isHeld = false;

    // Objenin oyunun başındaki "doğru duruş" rotasyonu (dünya uzayında).
    // Elde tutulurken rotasyon ne kadar garipleşirse garipleşsin, bırakınca hep buna dönecek.
    private Quaternion uprightRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        uprightRotation = transform.rotation; // sahnede editörde elle ayarladığın "doğru" duruş
    }

    public string GetInteractPrompt()
    {
        return isHeld ? $"E - {itemName} bırak" : $"E - {itemName} al";
    }

    public void Interact(PlayerInteraction player)
    {
        if (!isHeld)
            PickUp(player);
        else
            Drop(player);
    }

    private void PickUp(PlayerInteraction player)
    {
        holdPoint = player.HoldPoint;

        rb.isKinematic = true;      // fizik motoru artık objeyi hareket ettirmesin
        rb.useGravity = false;
        col.enabled = false;         // player'a çarpıp titremesin

        // SetParent YOK - parent hiç değişmiyor, Update() içinde world position/rotation takip edilecek
        isHeld = true;

        player.SetHeldItem(this);
    }

    public void Drop(PlayerInteraction player)
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        col.enabled = true;

        isHeld = false;
        holdPoint = null;
        player.SetHeldItem(null);
        // Obje şu an nerede duruyorsa (world space) oradan fizikle serbest düşer
    }

    /// <summary>
    /// CounterSurface tarafından çağrılır — objeyi raycast'in çarptığı tam noktaya bırakır.
    /// SetParent yok, sadece world position/rotation set ediliyor.
    /// </summary>
    public void PlaceOnCounter(Vector3 worldPosition)
    {
        transform.position = worldPosition + Vector3.up * surfaceYOffset;
        transform.rotation = uprightRotation; // her zaman doğru, dik açıyla otur

        rb.isKinematic = true;   // tezgahın üstünde sabit dursun, kaymasın
        rb.useGravity = false;
        col.enabled = true;

        isHeld = false;
    }

    private void Update()
    {
        // Elde tutarken world-space pozisyon/rotasyonu yumuşak şekilde HoldPoint'e (+ offset) kilitle.
        // Parent DEĞİŞMİYOR, bu yüzden localScale asla bozulmuyor.
        if (isHeld && holdPoint != null)
        {
            Vector3 targetPosition = holdPoint.TransformPoint(holdPositionOffset);
            Quaternion targetRotation = holdPoint.rotation * Quaternion.Euler(holdRotationOffsetEuler);

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * holdSmoothSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * holdSmoothSpeed);
        }
    }

    public bool IsHeld => isHeld;
}