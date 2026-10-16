using UnityEngine;

/// <summary>
/// Belirli bir item'ı belirli bir dock noktasına yerleştirir.
///
/// Örnek:
/// - PortafilterDock
/// - CupDock
/// - TamperDock
///
/// Sağ ve sol elde tutulan item'ları destekler.
/// Brew gibi işlemler sırasında dock kilitlenebilir.
///
/// Dock'a item başarıyla takıldığında opsiyonel takma sesi çalar.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MachineDockPoint : MonoBehaviour, IInteractable
{
    [Header("Kabul Edilen Item")]
    [Tooltip("Buraya sadece bu isimdeki item takılabilir.")]
    [SerializeField] private string acceptedItemName = "Portafilter";


    [Header("Dock Pozisyonu")]
    [Tooltip("Item dock'a oturduğunda kullanılacak pozisyon ofseti.")]
    [SerializeField] private Vector3 dockedLocalPositionOffset = Vector3.zero;

    [Tooltip("Item'ın doğal rotasyonuna eklenecek ekstra rotasyon.")]
    [SerializeField] private Vector3 dockedExtraRotationEuler = Vector3.zero;


    [Header("Gereksinimler")]
    [Tooltip("İşaretliyse item içinde ground coffee olmak zorunda.")]
    [SerializeField] private bool requireGroundCoffee = false;

    [Tooltip("İşaretliyse item tamp edilmiş olmak zorunda.")]
    [SerializeField] private bool requireTamped = false;

    [Tooltip("İşaretliyse kullanılmış kahveli item buraya takılamaz.")]
    [SerializeField] private bool rejectUsedCoffee = false;


    [Header("Takma Sesi")]
    [Tooltip("Item dock'a başarıyla takıldığında çalacak ses.")]
    [SerializeField] private AudioClip attachSound;

    [SerializeField]
    [Range(0f, 1f)]
    private float attachSoundVolume = 1f;


    // =========================================================
    // DURUM
    // =========================================================

    private bool isOccupied = false;

    private PickupItem dockedItem;

    private bool isLocked = false;


    // =========================================================
    // SES
    // =========================================================

    private AudioSource runtimeAudioSource;


    // =========================================================
    // DIŞARIDAN OKUNANLAR
    // =========================================================

    public bool IsOccupied =>
        isOccupied;

    public PickupItem DockedItem =>
        dockedItem;

    public bool IsLocked =>
        isLocked;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        CreateAudioSource();
    }


    // =========================================================
    // AUDIO SOURCE OLUŞTUR
    // =========================================================

    private void CreateAudioSource()
    {
        runtimeAudioSource =
            gameObject.AddComponent<AudioSource>();

        runtimeAudioSource.playOnAwake = false;

        runtimeAudioSource.loop = false;

        // 2D ses
        runtimeAudioSource.spatialBlend = 0f;

        runtimeAudioSource.volume =
            attachSoundVolume;

        runtimeAudioSource.mute = false;

        runtimeAudioSource.priority = 128;
    }


    // =========================================================
    // PROMPT
    // =========================================================

    public string GetInteractPrompt()
    {
        if (isLocked)
            return "Meşgul...";


        if (isOccupied)
            return $"E - {acceptedItemName} çıkar";


        return $"E - {acceptedItemName} tak";
    }


    // =========================================================
    // INTERACT
    // =========================================================

    public void Interact(PlayerInteraction player)
    {
        if (player == null)
            return;


        // Kilitliyse hiçbir şey yapılamaz.
        if (isLocked)
            return;


        // -----------------------------------------------------
        // DOCK DOLUYSA
        // -----------------------------------------------------

        if (isOccupied)
        {
            if (dockedItem != null)
            {
                dockedItem.ForcePickUp(player);
            }


            isOccupied = false;

            dockedItem = null;

            return;
        }


        // -----------------------------------------------------
        // OYUNCUNUN ELİNDEKİ ITEM'I BUL
        // -----------------------------------------------------

        PickupItem held = null;

        bool isLeftHandItem = false;


        // Önce sağ eldeki item'ı kontrol et.
        PickupItem rightHandItem =
            player.GetHeldItem();


        if (rightHandItem != null &&
            rightHandItem.ItemName.ToLower() ==
            acceptedItemName.ToLower())
        {
            held = rightHandItem;

            isLeftHandItem = false;
        }
        else
        {
            // Sonra sol eldeki item'ı kontrol et.
            PickupItem leftHandItem =
                player.GetLeftHeldItem();


            if (leftHandItem != null &&
                leftHandItem.ItemName.ToLower() ==
                acceptedItemName.ToLower())
            {
                held = leftHandItem;

                isLeftHandItem = true;
            }
        }


        // Uygun item bulunamadı.
        if (held == null)
            return;


        // -----------------------------------------------------
        // GROUND COFFEE KONTROLÜ
        // -----------------------------------------------------

        if (requireGroundCoffee &&
            !held.HasGroundCoffee)
        {
            return;
        }


        // -----------------------------------------------------
        // TAMP KONTROLÜ
        // -----------------------------------------------------

        if (requireTamped &&
            !held.IsTamped)
        {
            return;
        }


        // -----------------------------------------------------
        // USED COFFEE KONTROLÜ
        // -----------------------------------------------------

        if (rejectUsedCoffee &&
            held.HasUsedCoffee)
        {
            Debug.Log(
                "Bu item içinde kullanılmış kahve var."
            );

            return;
        }


        // -----------------------------------------------------
        // DOCK POZİSYONU
        // -----------------------------------------------------

        Vector3 worldPos =
            transform.position +
            transform.TransformDirection(
                dockedLocalPositionOffset
            );


        held.DockAt(
            worldPos,
            dockedExtraRotationEuler
        );


        // -----------------------------------------------------
        // DOCK DURUMU
        // -----------------------------------------------------

        dockedItem = held;

        isOccupied = true;


        // -----------------------------------------------------
        // HANGİ ELDEYSE ONU BOŞALT
        // -----------------------------------------------------

        if (isLeftHandItem)
        {
            player.SetLeftHeldItem(null);
        }
        else
        {
            player.SetHeldItem(null);
        }


        // -----------------------------------------------------
        // TAKMA SESİ
        // -----------------------------------------------------

        PlayAttachSound();
    }


    // =========================================================
    // TAKMA SESİ
    // =========================================================

    private void PlayAttachSound()
    {
        if (attachSound == null)
        {
            return;
        }


        if (runtimeAudioSource == null)
        {
            CreateAudioSource();
        }


        runtimeAudioSource.volume =
            attachSoundVolume;

        runtimeAudioSource.mute = false;

        runtimeAudioSource.spatialBlend = 0f;


        runtimeAudioSource.PlayOneShot(
            attachSound,
            attachSoundVolume
        );


        Debug.Log(
            "DOCK TAKMA SESI CALDI! " +
            gameObject.name
        );
    }


    // =========================================================
    // LOCK / UNLOCK
    // =========================================================

    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }


    // =========================================================
    // GİZMO
    // =========================================================

    private void OnDrawGizmos()
    {
        if (isLocked)
        {
            Gizmos.color = Color.yellow;
        }
        else if (isOccupied)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.cyan;
        }


        Gizmos.DrawWireSphere(
            transform.position,
            0.03f
        );
    }
}