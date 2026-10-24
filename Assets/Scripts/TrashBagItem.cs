using UnityEngine;

/// <summary>
/// Çöp poşeti item'ı. İki hali var:
/// - Empty: kovaya takılacak yeni poşet ya da poşet ruloları.
///          "Uses" kadar kez kovaya poşet takılabilir, bitince yok olur.
///          (Tek poşet = 1, rulo = 6)
/// - Full : kovadan çıkarılan, dışarı götürülecek dolu poşet.
///          (Uses burada kullanılmaz)
/// </summary>
[RequireComponent(typeof(PickupItem))]
public class TrashBagItem : MonoBehaviour
{
    public enum BagState
    {
        Empty,
        Full
    }

    [SerializeField] private BagState state = BagState.Empty;

    [Tooltip("Sadece Empty için: bu item'dan kaç kez poşet takılabilir. Rulo için 6.")]
    [SerializeField] private int uses = 1;

    public BagState State => state;

    public int UsesRemaining => uses;

    // Dolu poşetin içinde kaç şey olduğu (ileride puan/istatistik için).
    public int ItemCount { get; private set; }

    public void SetItemCount(int count)
    {
        ItemCount = Mathf.Max(0, count);
    }

    // Bir kullanım harca. Kullanım kalmadıysa false döner.
    public bool ConsumeUse()
    {
        if (uses <= 0)
            return false;

        uses--;

        return true;
    }
}