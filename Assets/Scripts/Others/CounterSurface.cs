using UnityEngine;

/// <summary>
/// Tezgahın collider'ı olan yüzeyine eklenir.
/// Oyuncu elindeki item'ı baktığı noktaya bırakır.
///
/// Shot bardağı tezgaha bırakılırsa drop sesi burada çalınır.
/// DockPoint'lere bırakıldığında bu ses ÇALMAZ.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CounterSurface : MonoBehaviour, IInteractable
{
    [Header("Yerleştirme Ayarları")]

    [Tooltip("Başka bir item'a bundan daha yakın noktaya bırakmayı engeller.")]
    [SerializeField] private float minDistanceBetweenItems = 0.15f;

    [Tooltip("Tezgah üstündeki item'ların layer'ı.")]
    [SerializeField] private LayerMask itemLayer;

    [Tooltip("Tezgah üstündeki makinelerin bulunduğu layer.")]
    [SerializeField] private LayerMask obstacleLayer;


    // =========================================================
    // SHOT BARDAĞI SESİ
    // =========================================================

    [Header("Shot Bardagi Ses")]

    [Tooltip("Shot bardağını tezgaha bırakınca çalacak ses.")]
    [SerializeField] private AudioClip shotGlassDropClip;

    [SerializeField]
    [Range(0f, 1f)]
    private float shotGlassDropVolume = 1f;


    // =========================================================
    // AUDIO SOURCE
    // =========================================================

    private AudioSource audioSource;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }


    // =========================================================
    // PROMPT
    // =========================================================

    public string GetInteractPrompt()
    {
        return "E - Tezgaha bırak";
    }


    // =========================================================
    // INTERACT
    // =========================================================

    public void Interact(PlayerInteraction player)
    {
        if (player == null)
            return;


        // =====================================================
        // SAĞ EL
        // =====================================================

        PickupItem held = player.GetHeldItem();

        bool fromLeftHand = false;


        // =====================================================
        // SAĞ EL BOŞSA SOL EL
        // =====================================================

        if (held == null)
        {
            held = player.GetLeftHeldItem();
            fromLeftHand = true;
        }


        // İki el de boş
        if (held == null)
            return;


        // =====================================================
        // BIRAKILACAK NOKTA
        // =====================================================

        Vector3 placePos = player.LastHitPoint;


        // =====================================================
        // NOKTA DOLU MU?
        // =====================================================

        if (IsSpotOccupied(placePos))
            return;


        // =====================================================
        // SHOT BARDAĞI MI?
        //
        // Bırakmadan ÖNCE kontrol ediyoruz.
        // Çünkü PlaceOnCounter sonrası artık elde olmayacak.
        // =====================================================

        PourSource pourSource =
            held.GetComponent<PourSource>();


        bool isShotGlass =
            pourSource != null;


        // =====================================================
        // TEZGAHA BIRAK
        // =====================================================

        held.PlaceOnCounter(placePos);


        // =====================================================
        // ELİ TEMİZLE
        // =====================================================

        if (fromLeftHand)
        {
            player.SetLeftHeldItem(null);
        }
        else
        {
            player.SetHeldItem(null);
        }


        // =====================================================
        // SHOT BARDAĞI BIRAKMA SESİ
        //
        // SADECE TEZGAHA BIRAKILDIĞINDA ÇALIŞIR.
        // =====================================================

        if (isShotGlass)
        {
            PlayShotGlassDropSound();
        }
    }


    // =========================================================
    // SHOT DROP SESİ
    // =========================================================

    private void PlayShotGlassDropSound()
    {
        if (shotGlassDropClip == null)
        {
            Debug.LogWarning(
                "CounterSurface: Shot Glass Drop Clip atanmemiş!"
            );

            return;
        }


        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                audioSource =
                    gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
        }


        audioSource.PlayOneShot(
            shotGlassDropClip,
            shotGlassDropVolume
        );


        Debug.Log(
            "SHOT BARDAĞI TEZGAHA BIRAKILDI → DROP SESİ ÇALDI"
        );
    }


    // =========================================================
    // NOKTA DOLULUK KONTROLÜ
    // =========================================================

    private bool IsSpotOccupied(Vector3 pos)
    {
        Vector3 checkCenter =
            pos + Vector3.up * 0.05f;


        // =====================================================
        // ITEM KONTROLÜ
        // =====================================================

        Collider[] nearbyItems =
            Physics.OverlapSphere(
                checkCenter,
                minDistanceBetweenItems,
                itemLayer
            );


        foreach (Collider hitCollider in nearbyItems)
        {
            PickupItem item =
                hitCollider.GetComponentInParent<PickupItem>();


            if (item != null && !item.IsHeld)
            {
                return true;
            }
        }


        // =====================================================
        // MAKİNE / ENGEL KONTROLÜ
        // =====================================================

        Collider[] nearbyObstacles =
            Physics.OverlapSphere(
                checkCenter,
                minDistanceBetweenItems,
                obstacleLayer
            );


        if (nearbyObstacles.Length > 0)
        {
            return true;
        }


        return false;
    }
}