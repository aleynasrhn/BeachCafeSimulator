using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Kafe zeminindeki pislik havuzu. Sen sahneye 10 tane pislik
/// objesi koyacaksın (hepsi başta kapalı duracak), her doğru
/// sipariş tesliminde bunlardan rastgele BİR tanesi aktif olur.
/// Mop ile süpürülerek temizlenir (SweepableStain kullanır).
/// </summary>
public class CafeFloorLitterZone : MonoBehaviour, ILitterZone
{
    public static CafeFloorLitterZone Instance { get; private set; }

    [Header("Kafe Zemini - Olası Pislikler (10 tane öner)")]
    [Tooltip("Hepsi sahnede kapalı (inactive) dursun, script başta zaten kapatır.")]
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

    // =========================================================
    // HER DOĞRU SİPARİŞ TESLİMİNDE BİR PİSLİK ÇIKAR
    // =========================================================

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
}