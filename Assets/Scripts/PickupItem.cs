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

    [Header("Kahve Doldurma (sadece portafilter için, diğer itemlerde boş bırak)")]
    [Tooltip("portafilter.001 > GroundCoffee objesini buraya sürükle. Oyun başında otomatik gizlenir, grinder'da doldurulunca görünür.")]
    [SerializeField] private GameObject groundCoffeeVisual;

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

        if (groundCoffeeVisual != null)
            groundCoffeeVisual.SetActive(false); // oyun başında kahve gizli
    }

    /// <summary>CoffeeGrinderInteraction tarafından çağrılır - portafilterin içini doldurur.</summary>
    public void FillWithGroundCoffee()
    {
        if (groundCoffeeVisual != null)
            groundCoffeeVisual.SetActive(true);
    }

    /// <summary>İleride "kahveyi boşalt/temizle" mekaniği için hazır dursun.</summary>
    public void EmptyGroundCoffee()
    {
        if (groundCoffeeVisual != null)
            groundCoffeeVisual.SetActive(false);
    }

    public bool HasGroundCoffee => groundCoffeeVisual != null && groundCoffeeVisual.activeSelf;

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

    /// <summary>
    /// MachineDockPoint gibi tam kontrollü yerleşmeler için — surfaceYOffset ya da
    /// uprightRotation uygulamaz, verilen pozisyon/rotasyonu olduğu gibi kullanır
    /// (makineye takılırken belirli bir açıda oturması gerekebilir).
    /// </summary>
    public void PlaceAtExact(Vector3 worldPosition, Quaternion worldRotation)
    {
        transform.position = worldPosition;
        transform.rotation = worldRotation;

        rb.isKinematic = true;
        rb.useGravity = false;
        col.enabled = true;

        isHeld = false;
    }

    /// <summary>
    /// MachineDockPoint gibi başka bir sistemin, item'ı dock'tan çıkarıp
    /// oyuncunun eline zorla vermesi için (normal PickUp() private, bu public wrapper).
    /// </summary>
    public void ForcePickUp(PlayerInteraction player)
    {
        holdPoint = player.HoldPoint;

        rb.isKinematic = true;
        rb.useGravity = false;
        col.enabled = false;

        isHeld = true;
        player.SetHeldItem(this);
    }

    /// <summary>
    /// MachineDockPoint tarafından çağrılır. PlaceAtExact'ten farkı: dock noktasının
    /// KENDİ rotasyonunu değil, itemin doğal/orijinal duruşunu (uprightRotation) kullanır,
    /// üstüne sadece verilen ek açıyı ekler. Böylece dock noktasını boş bir GameObject
    /// olarak (rotasyonuyla hiç uğraşmadan) oluşturabilirsin, item hep doğru şekilde görünür.
    /// </summary>
    public void DockAt(Vector3 worldPosition, Vector3 extraRotationEuler)
    {
        transform.position = worldPosition;
        transform.rotation = uprightRotation * Quaternion.Euler(extraRotationEuler);

        rb.isKinematic = true;
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
    public string ItemName => itemName;
}