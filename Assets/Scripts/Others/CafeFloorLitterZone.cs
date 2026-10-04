using System.Collections.Generic;
using UnityEngine;

public class CafeFloorLitterZone : MonoBehaviour, ILitterZone
{
    public static CafeFloorLitterZone Instance { get; private set; }

    [Header("Kafe Zemini - Olası Pislikler (10 tane öner)")]
    [SerializeField] private List<GameObject> litterPool = new List<GameObject>();

    private readonly List<GameObject> activeLitter = new List<GameObject>();

    public bool IsDirty => activeLitter.Count > 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        foreach (GameObject obj in litterPool)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void MakeDirty()
    {
        List<GameObject> available = new List<GameObject>();

        foreach (GameObject obj in litterPool)
        {
            if (obj != null && !obj.activeSelf)
                available.Add(obj);
        }

        if (available.Count == 0)
        {
            Debug.Log(
                "CafeFloorLitterZone: tüm pislikler zaten aktif, " +
                "yeni pislik eklenemedi."
            );

            return;
        }

        GameObject chosen =
            available[Random.Range(0, available.Count)];

        chosen.SetActive(true);

        activeLitter.Add(chosen);

        Debug.Log(
            $"Kafe zemini kirlendi. Aktif kir: {activeLitter.Count}"
        );
    }

    public void NotifyLitterCleaned(GameObject obj)
    {
        activeLitter.Remove(obj);
    }

    // =========================================================
    // YENİ GÜN: TÜM KİRİ ANINDA TEMİZLE
    // =========================================================

    public void CleanAll()
    {
        foreach (GameObject obj in litterPool)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        activeLitter.Clear();
    }
}