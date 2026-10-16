using System.Collections.Generic;
using UnityEngine;

public class ToiletManager : MonoBehaviour
{
    public static ToiletManager Instance { get; private set; }

    [Header("Tuvalet Odası Kapısı")]
    [SerializeField] private ToiletDoor roomDoor;

    [Tooltip("Oda kapısının DIŞINDA, NavMesh üzerinde nokta. Mavi ok kapıya baksın.")]
    [SerializeField] private Transform roomEntryPoint;

    [Tooltip("Oda kapısından hemen İÇERİDE nokta.")]
    [SerializeField] private Transform roomInsidePoint;

    [Header("Kabinler")]
    [SerializeField]
    private List<ToiletStall> stalls =
        new List<ToiletStall>();

    [Header("Ortak Alan (Lavabo) Kirlenmesi")]
    [Tooltip("Lavabo/ortak alanın kir bölgesi (OrtakAlan objesindeki AreaLitterZone).")]
    [SerializeField] private AreaLitterZone commonAreaZone;

    [SerializeField] private int minUsesBeforeCommonDirty = 3;
    [SerializeField] private int maxUsesBeforeCommonDirty = 4;

    private int usesSinceLastCommonDirty = 0;
    private int nextCommonDirtyThreshold;

    public ToiletDoor RoomDoor => roomDoor;
    public Transform RoomEntryPoint => roomEntryPoint;
    public Transform RoomInsidePoint => roomInsidePoint;

    public int StallCount => stalls.Count;

    public bool IsCommonAreaDirty =>
        commonAreaZone != null && commonAreaZone.IsDirty;

    public int DirtyStallCount
    {
        get
        {
            int count = 0;

            foreach (ToiletStall stall in stalls)
            {
                if (stall != null && stall.IsDirty)
                    count++;
            }

            return count;
        }
    }

    private void Awake()
    {
        Instance = this;

        PickNextCommonDirtyThreshold();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void PickNextCommonDirtyThreshold()
    {
        nextCommonDirtyThreshold =
            Random.Range(
                minUsesBeforeCommonDirty,
                maxUsesBeforeCommonDirty + 1
            );
    }

    public ToiletStall TryReserveStall()
    {
        if (roomEntryPoint == null ||
            roomInsidePoint == null)
        {
            Debug.LogWarning(
                "ToiletManager: Oda kapısı/noktaları atanmamış!"
            );

            return null;
        }

        List<ToiletStall> available =
            new List<ToiletStall>();

        foreach (ToiletStall stall in stalls)
        {
            if (stall != null && stall.IsAvailable)
                available.Add(stall);
        }

        if (available.Count == 0)
            return null;

        ToiletStall selected =
            available[Random.Range(0, available.Count)];

        selected.TryReserve();

        return selected;
    }

    // ToiletStall.FinishUse() tarafından çağrılır.
    public void NotifyStallUsed()
    {
        usesSinceLastCommonDirty++;

        if (usesSinceLastCommonDirty >= nextCommonDirtyThreshold)
        {
            commonAreaZone?.MakeDirty();

            usesSinceLastCommonDirty = 0;

            PickNextCommonDirtyThreshold();
        }
    }
}