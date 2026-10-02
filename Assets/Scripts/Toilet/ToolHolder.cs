using UnityEngine;

/// <summary>
/// Duvara/aracın asıldığı yere konan nokta. Mop, süpürge gibi
/// Item Name'i belirtilen bir PickupItem için çalışır.
///
/// - Oyuncu elinde DOĞRU item'ı tutuyorsa ve buraya E ile basarsa:
///   item buraya SMOOTH şekilde (duration kadar sürede) yerleşir.
/// - Oyuncu eli boşsa ve burada zaten asılı bir item varsa:
///   item'ı eline alır.
/// - Oyuncu elinde YANLIŞ bir item tutuyorsa: hiçbir şey olmaz.
///
/// ÖNEMLİ: Item'ın mesh pivotu tam istediğin noktada değilse,
/// Dock Position Offset ile (bu objenin kendi local ekseninde)
/// ince ayar yapabilirsin. Rotasyon farkı için Extra Rotation Euler
/// kullan.
/// </summary>
public class ToolHolder : MonoBehaviour, IInteractable
{
    [Tooltip("PickupItem'daki Item Name alanıyla BİREBİR aynı olmalı. Örn: 'Mop'")]
    [SerializeField] private string acceptedItemName = "Mop";

    [Header("Asılma Pozisyonu")]
    [Tooltip("Item'ın buraya asılırken alacağı ekstra rotasyon (local Euler).")]
    [SerializeField] private Vector3 extraRotationEuler = Vector3.zero;

    [Tooltip("Item'ın tam olarak nereye oturacağını (bu objenin kendi dönmüş ekseninde) ince ayarlamak için. Mesh pivotu tam merkezde değilse kullan.")]
    [SerializeField] private Vector3 dockPositionOffset = Vector3.zero;

    [Header("Yerleştirme Animasyonu")]
    [Tooltip("Elden tutucuya yerleşirken geçen süre (saniye). Büyüdükçe daha yavaş/smooth olur.")]
    [SerializeField] private float dockDuration = 0.35f;

    // Şu an bu tutucuda asılı olan item (varsa).
    private PickupItem dockedItem;

    public bool HasItem => dockedItem != null;

    public string GetInteractPrompt()
    {
        if (dockedItem != null)
        {
            return $"{dockedItem.ItemName} al";
        }

        return $"{acceptedItemName} as";
    }

    public void Interact(PlayerInteraction player)
    {
        // ---------------------------------------------
        // DURUM 1: BURADA ZATEN BİR ITEM VAR → ELE AL
        // ---------------------------------------------

        if (dockedItem != null)
        {
            // Oyuncunun eli doluysa alamaz.
            if (player.GetHeldItem() != null)
            {
                Debug.Log("ToolHolder: Elin dolu, önce bıraksan iyi olur.");
                return;
            }

            PickupItem itemToTake = dockedItem;

            UnbindDockedItem();

            // ForcePickUp çağrılınca PickupItem.Update() içindeki
            // Lerp zaten elin hedefine doğru smooth hareket başlatır.
            itemToTake.ForcePickUp(player);

            return;
        }

        // ---------------------------------------------
        // DURUM 2: BURASI BOŞ → OYUNCUNUN ELİNDEKİNİ SMOOTH AS
        // ---------------------------------------------

        PickupItem held = player.GetHeldItem();

        if (held == null)
        {
            Debug.Log("ToolHolder: Elinde asılacak bir şey yok.");
            return;
        }

        if (held.ItemName != acceptedItemName)
        {
            Debug.Log(
                $"ToolHolder: Bu tutucu sadece '{acceptedItemName}' kabul ediyor."
            );

            return;
        }

        player.SetHeldItem(null);

        BindDockedItem(held);

        Vector3 targetWorldPosition =
            transform.TransformPoint(dockPositionOffset);

        held.SmoothDockAt(
            targetWorldPosition,
            extraRotationEuler,
            dockDuration
        );
    }

    // =========================================================
    // DOCKED ITEM TAKİBİ (DESYNC KORUMASI)
    // =========================================================
    //
    // Item buraya asılınca, PickupItem'ın OnPickedUp event'ine
    // abone oluyoruz. Item NE ŞEKİLDE alınırsa alınsın (bu
    // scriptin kendi ForcePickUp çağrısıyla ya da oyuncunun
    // objeye DOĞRUDAN bakıp E'ye basmasıyla) bu event tetiklenir
    // ve dockedItem referansı otomatik temizlenir. Böylece holder
    // hiçbir zaman "orada bir şey var" diye yanlış bilgi tutmaz.
    //
    // =========================================================

    private void BindDockedItem(PickupItem item)
    {
        dockedItem = item;

        item.OnPickedUp += HandleDockedItemPickedUp;
    }

    private void UnbindDockedItem()
    {
        if (dockedItem != null)
        {
            dockedItem.OnPickedUp -= HandleDockedItemPickedUp;
        }

        dockedItem = null;
    }

    private void HandleDockedItemPickedUp(PickupItem item)
    {
        if (item != dockedItem)
            return;

        dockedItem.OnPickedUp -= HandleDockedItemPickedUp;

        dockedItem = null;
    }

    private void OnDisable()
    {
        UnbindDockedItem();
    }

    // =========================================================
    // EDİTÖRDE GÖRMEK İÇİN (hedef nokta nerede, görmek kolaylaşsın)
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector3 point =
            transform.TransformPoint(dockPositionOffset);

        Gizmos.DrawWireSphere(point, 0.03f);
        Gizmos.DrawLine(transform.position, point);
    }
}