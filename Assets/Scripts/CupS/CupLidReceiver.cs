using UnityEngine;

public class CupLidReceiver : MonoBehaviour
{
    [Header("Kapak Takma Noktası")]
    [SerializeField] private Transform lidPoint;

    [Header("Bardak Boyutu")]
    [SerializeField] private CupSize cupSize;

    [Header("UI")]
    [SerializeField] private InteractionUI interactionUI;

    private GameObject attachedLid;

    public CupSize CupSize => cupSize;

    public bool HasLid =>
        attachedLid != null;


    // =========================================================
    // BAŞLANGIÇ
    // =========================================================

    private void Awake()
    {
        // Prefab içinden sahnedeki InteractionUI'yi bul
        if (interactionUI == null)
        {
            interactionUI =
                FindFirstObjectByType<InteractionUI>();
        }

        if (interactionUI == null)
        {
            Debug.LogWarning(
                "CupLidReceiver: Sahnedeki InteractionUI bulunamadı!"
            );
        }
    }


    // =========================================================
    // KAPAK TAK
    // =========================================================

    public bool TryAttachLid(PickupItem lidPickup)
    {
        if (lidPickup == null)
            return false;


        // =====================================================
        // KAPAK KONTROLÜ
        // =====================================================

        LidItem lidItem =
            lidPickup.GetComponent<LidItem>();

        if (lidItem == null)
            return false;


        // =====================================================
        // BOYUT KONTROLÜ
        // =====================================================

        if (lidItem.Size != cupSize)
        {
            interactionUI?.ShowMessage(
                "Bu kapak bu bardağa uygun değil!"
            );

            return false;
        }


        // =====================================================
        // ZATEN KAPAK VAR MI?
        // =====================================================

        if (HasLid)
        {
            interactionUI?.ShowMessage(
                "Bu bardağın zaten kapağı var!"
            );

            return false;
        }


        // =====================================================
        // LID POINT KONTROLÜ
        // =====================================================

        if (lidPoint == null)
        {
            Debug.LogWarning(
                "CupLidReceiver: LidPoint atanmadı!",
                this
            );

            return false;
        }


        // =====================================================
        // KAPAĞI TAK
        // =====================================================

        attachedLid =
            lidPickup.gameObject;

        lidPickup.AttachToObject(
            transform,
            lidPoint.position,
            lidPoint.rotation
        );


        Debug.Log(
            "Kapak cup'a takıldı."
        );


        return true;
    }


    // =========================================================
    // KAPAK ÇIKAR
    // =========================================================

    public void DetachLid(PickupItem lidPickup)
    {
        if (lidPickup == null)
            return;


        // Sadece gerçekten bu cup'a bağlı olan
        // kapağı temizle.
        if (attachedLid ==
            lidPickup.gameObject)
        {
            attachedLid = null;

            Debug.Log(
                "Kapak cup'tan çıkarıldı."
            );
        }
    }
}