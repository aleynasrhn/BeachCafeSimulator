using UnityEngine;

public class ToiletStall : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private ToiletDoor door;

    [Tooltip("Kabin kapısının ÖNÜNDE durulacak nokta. Mavi ok kapıya baksın.")]
    [SerializeField] private Transform entryPoint;

    [Tooltip("Kabinin içinde durulacak nokta.")]
    [SerializeField] private Transform insidePoint;

    [Tooltip("Bu kabinin kir bölgesi (aynı objede olabilir).")]
    [SerializeField] private AreaLitterZone litterZone;

    public ToiletDoor Door => door;
    public Transform EntryPoint => entryPoint;
    public Transform InsidePoint => insidePoint;

    public bool IsReserved { get; private set; }

    public bool IsDirty =>
        litterZone != null && litterZone.IsDirty;

    public bool IsAvailable =>
        !IsReserved && !IsDirty;

    public bool TryReserve()
    {
        if (!IsAvailable)
            return false;

        IsReserved = true;

        if (door != null)
            door.SetLocked(true);

        return true;
    }

    // NPC yolda vazgeçerse (örn. yol bulunamadı) çağrılır.
    public void Release()
    {
        IsReserved = false;

        if (door != null)
            door.SetLocked(false);
    }

    // NPC gerçekten kullanıp çıktığında çağrılır.
    public void FinishUse()
    {
        IsReserved = false;

        if (door != null)
            door.SetLocked(false);

        litterZone?.MakeDirty();

        if (ToiletManager.Instance != null)
        {
            ToiletManager.Instance.NotifyStallUsed();
        }
    }
}