using System.Collections.Generic;
using UnityEngine;

public class AreaLitterZone : MonoBehaviour, ILitterZone
{
    [Header("Bu Bölgedeki Olası Kirler (Kağıt + Leke)")]
    [SerializeField] private List<GameObject> litterPool = new List<GameObject>();

    [Header("Kirlenince Kaç Tanesi Aktif Olsun")]
    [SerializeField] private int minLitterCount = 2;
    [SerializeField] private int maxLitterCount = 4;

    private readonly List<GameObject> activeLitter = new List<GameObject>();

    public bool IsDirty => activeLitter.Count > 0;

    private void Awake()
    {
        foreach (GameObject obj in litterPool)
        {
            if (obj != null)
                obj.SetActive(false);
        }
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
            Debug.LogWarning(
                $"{name}: Litter Pool'daki tüm objeler zaten aktif " +
                "(ya da liste boş), kirlenilemedi."
            );

            return;
        }

        int count = Mathf.Clamp(
            Random.Range(minLitterCount, maxLitterCount + 1),
            1,
            available.Count
        );

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, available.Count);
            GameObject chosen = available[index];

            chosen.SetActive(true);
            activeLitter.Add(chosen);

            available.RemoveAt(index);
        }

        Debug.Log($"{name} kirlendi. Aktif kir: {activeLitter.Count}");
    }

    public void NotifyLitterCleaned(GameObject obj)
    {
        activeLitter.Remove(obj);

        if (!IsDirty)
        {
            Debug.Log($"{name} tamamen temizlendi.");
        }
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