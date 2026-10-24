using UnityEngine;

/// <summary>
/// Kafe ve tuvaletlerdeki çöp kovası. Eski CoffeeTrashBin ve LitterBin
/// scriptlerinin yerine geçer.
///
/// - Poşet takılıyken içine eşya atılabilir (bardak, kapak, kutu,
///   tuvalet çöpü, portafilter telvesi). Her biri 1 yer kaplar.
/// - Kapasite dolunca (7-8) yeni bir şey atılamaz, ekrana mesaj çıkar.
/// - Eller boşken E: poşet çıkarılır, dolu poşet ele gelir.
/// - Poşetsiz kovaya hiçbir şey atılamaz. Elinde poşet (ya da poşet
///   rulosu) varken E ile yeni poşet takılır; rulo kullanımı bitince
///   yok olur.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TrashBin : MonoBehaviour, IInteractable
{
    [Header("Kapasite (her yeni poşette bu aralıktan rastgele seçilir)")]
    [SerializeField] private int minCapacity = 7;
    [SerializeField] private int maxCapacity = 8;

    [Header("Poşet")]
    [Tooltip("İşaretliyse oyun başında kovada hazır bir poşet vardır.")]
    [SerializeField] private bool startsWithBag = true;

    [Tooltip("Kovadan çıkarılınca ele gelecek DOLU çöp poşeti prefab'ı.")]
    [SerializeField] private GameObject fullBagPrefab;

    [Tooltip("Dolu poşetin doğacağı nokta. Boşsa kovanın biraz üstü kullanılır.")]
    [SerializeField] private Transform bagSpawnPoint;

    [Header("Görünüm (Opsiyonel)")]
    [Tooltip("Poşet takılıyken görünen obje (kovanın içindeki poşet).")]
    [SerializeField] private GameObject bagVisual;

    [Tooltip("Kova dolunca görünen obje (taşan çöp gibi).")]
    [SerializeField] private GameObject fullIndicator;

    [Header("Portafilter")]
    [SerializeField] private string portafilterItemName = "Portafilter";

    [Tooltip("Portafilter telvesini boşaltmak kovada kaç yer kaplasın? 0 = hiç.")]
    [SerializeField] private int portafilterFillCost = 1;

    [Header("Ekran Mesajı")]
    [Tooltip("Boş bırakırsan sahnede otomatik bulunur.")]
    [SerializeField] private InteractionUI interactionUI;

    private bool hasBag = false;
    private int currentCount = 0;
    private int currentCapacity = 8;

    public bool HasBag => hasBag;

    public bool IsFull =>
        hasBag && currentCount >= currentCapacity;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (startsWithBag)
        {
            InstallBag();
        }
        else
        {
            UpdateVisuals();
        }
    }

    // =========================================================
    // PROMPT
    // =========================================================

    public string GetInteractPrompt()
    {
        if (!hasBag)
            return "E - Çöp kovası (poşet yok)";

        if (IsFull)
            return $"Çöp dolu ({currentCount}/{currentCapacity}) - boş elle E: poşeti çıkar";

        return $"E - Çöp kovası ({currentCount}/{currentCapacity})";
    }

    // =========================================================
    // ETKİLEŞİM
    // =========================================================

    public void Interact(PlayerInteraction player)
    {
        if (player == null)
            return;

        PickupItem held =
            GetHeld(player, out bool fromLeftHand);

        // -----------------------------------------------------
        // ELLER BOŞ: POŞETİ ÇIKAR
        // -----------------------------------------------------

        if (held == null)
        {
            TryTakeOutBag(player);

            return;
        }

        // -----------------------------------------------------
        // ELDE ÇÖP POŞETİ / RULOSU
        // -----------------------------------------------------

        TrashBagItem bagItem =
            held.GetComponent<TrashBagItem>();

        if (bagItem != null)
        {
            if (bagItem.State == TrashBagItem.BagState.Empty)
            {
                TryInstallBag(player, held, bagItem, fromLeftHand);
            }
            else
            {
                Say(
                    "Dolu çöp poşetini dışarıdaki çöp konteynerine at."
                );
            }

            return;
        }

        // -----------------------------------------------------
        // DİĞER EŞYALAR: ÇÖPE AT
        // -----------------------------------------------------

        TryThrowAway(player, held, fromLeftHand);
    }

    // =========================================================
    // EŞYA AT
    // =========================================================

    private void TryThrowAway(
        PlayerInteraction player,
        PickupItem held,
        bool fromLeftHand)
    {
        bool isPortafilter =
            held.ItemName == portafilterItemName;

        int cost = 1;

        if (isPortafilter)
        {
            if (!held.HasUsedCoffee)
            {
                Say("Portafilter'da boşaltılacak kullanılmış kahve yok.");

                return;
            }

            cost = portafilterFillCost;
        }
        else if (!IsThrowable(held))
        {
            Say("Bu eşya çöpe atılamaz.");

            return;
        }

        if (!hasBag)
        {
            Say("Kovada çöp poşeti yok. Yeni bir poşet takman lazım.");

            return;
        }

        if (currentCount >= currentCapacity)
        {
            Say("Çöp kovası dolu! Boş elle E'ye basıp poşeti çıkar.");

            return;
        }

        // -----------------------------------------------------
        // UYGULA
        // -----------------------------------------------------

        if (isPortafilter)
        {
            held.EmptyGroundCoffee();
        }
        else
        {
            ClearHand(player, fromLeftHand);

            LitterMarker marker =
                held.GetComponent<LitterMarker>();

            if (marker != null)
            {
                // Tuvalet çöpü: kapat, bölgeye haber ver.
                held.ReturnToOriginalPosition();
                held.gameObject.SetActive(false);

                marker.Zone?.NotifyLitterCleaned(held.gameObject);
            }
            else
            {
                Destroy(held.gameObject);
            }
        }

        currentCount += cost;

        UpdateVisuals();

        Debug.Log(
            $"{name}: çöp atıldı ({currentCount}/{currentCapacity})."
        );
    }

    private bool IsThrowable(PickupItem held)
    {
        return held.GetComponent<DrinkRecipe>() != null ||
               held.GetComponent<LidItem>() != null ||
               held.GetComponent<LitterMarker>() != null ||
               held.GetComponent<TrashableItem>() != null;
    }

    // =========================================================
    // POŞET TAK (RULODAN BİR KULLANIM HARCAR)
    // =========================================================

    private void TryInstallBag(
        PlayerInteraction player,
        PickupItem bagPickup,
        TrashBagItem bagItem,
        bool fromLeftHand)
    {
        // Kovada zaten poşet varsa rulodan kullanım harcama.
        if (hasBag)
        {
            Say("Kovada zaten çöp poşeti var.");

            return;
        }

        if (!bagItem.ConsumeUse())
        {
            // Kullanımı kalmamış (Inspector'da Uses = 0 gibi),
            // elde boşuna durmasın.
            ClearHand(player, fromLeftHand);

            Destroy(bagPickup.gameObject);

            Say("Bu poşet bitmiş.");

            return;
        }

        InstallBag();

        if (bagItem.UsesRemaining <= 0)
        {
            ClearHand(player, fromLeftHand);

            Destroy(bagPickup.gameObject);

            Say("Yeni çöp poşeti takıldı. Poşetler bitti.");
        }
        else
        {
            Say(
                $"Yeni çöp poşeti takıldı. " +
                $"{bagItem.UsesRemaining} poşet kaldı."
            );
        }
    }

    private void InstallBag()
    {
        hasBag = true;

        currentCount = 0;

        int lo = Mathf.Min(minCapacity, maxCapacity);
        int hi = Mathf.Max(minCapacity, maxCapacity);

        currentCapacity = Random.Range(lo, hi + 1);

        UpdateVisuals();
    }

    // =========================================================
    // POŞETİ ÇIKAR
    // =========================================================

    private void TryTakeOutBag(PlayerInteraction player)
    {
        if (!hasBag)
        {
            Say("Kovada çöp poşeti yok. Yeni bir poşet takman lazım.");

            return;
        }

        if (currentCount <= 0)
        {
            Say("Çöp kovası zaten boş.");

            return;
        }

        if (fullBagPrefab == null)
        {
            Debug.LogError(
                $"{name}: Full Bag Prefab atanmamış!"
            );

            Say("Dolu poşet prefab'ı atanmamış (Inspector).");

            return;
        }

        Vector3 spawnPosition =
            bagSpawnPoint != null
                ? bagSpawnPoint.position
                : transform.position + Vector3.up * 0.6f;

        GameObject bagObject =
            Instantiate(
                fullBagPrefab,
                spawnPosition,
                Quaternion.identity
            );

        PickupItem pickup =
            bagObject.GetComponent<PickupItem>();

        if (pickup == null)
        {
            Debug.LogError(
                $"{name}: Full Bag Prefab'ında PickupItem yok!"
            );

            Destroy(bagObject);

            return;
        }

        TrashBagItem bagItem =
            bagObject.GetComponent<TrashBagItem>();

        bagItem?.SetItemCount(currentCount);

        pickup.ForcePickUp(player);

        if (!pickup.IsHeld)
        {
            Destroy(bagObject);

            Say("Poşet ele alınamadı.");

            return;
        }

        hasBag = false;

        currentCount = 0;

        UpdateVisuals();

        Say("Çöp poşeti çıkarıldı. Dışarıdaki çöp konteynerine götür.");
    }

    // =========================================================
    // YARDIMCILAR
    // =========================================================

    private PickupItem GetHeld(
        PlayerInteraction player,
        out bool fromLeftHand)
    {
        fromLeftHand = false;

        PickupItem held =
            player.GetHeldItem();

        if (held != null)
            return held;

        held =
            player.GetLeftHeldItem();

        if (held != null)
        {
            fromLeftHand = true;

            return held;
        }

        return null;
    }

    private void ClearHand(
        PlayerInteraction player,
        bool fromLeftHand)
    {
        if (fromLeftHand)
        {
            player.SetLeftHeldItem(null);
        }
        else
        {
            player.SetHeldItem(null);
        }
    }

    private void UpdateVisuals()
    {
        if (bagVisual != null)
        {
            bagVisual.SetActive(hasBag);
        }

        if (fullIndicator != null)
        {
            fullIndicator.SetActive(IsFull);
        }
    }

    private void Say(string message)
    {
        Debug.Log($"{name}: {message}");

        if (interactionUI == null)
        {
            interactionUI =
                FindFirstObjectByType<InteractionUI>();
        }

        interactionUI?.ShowMessage(message);
    }
}