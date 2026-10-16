using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC Prefabları")]
    [SerializeField]
    private List<GameObject> npcPrefabs =
        new List<GameObject>();

    [Header("Spawn Noktaları")]
    [SerializeField]
    private Transform[] spawnPoints;

    [Header("Aktif NPC Ayarları")]
    [SerializeField]
    private int maxActiveNPC = 12;

    [Header("Spawn Süresi (Gün başında DayCycleManager ezer)")]
    [SerializeField]
    private float minSpawnInterval = 5f;

    [SerializeField]
    private float maxSpawnInterval = 10f;

    [Header("Günlük Müşteri (Gün başında DayCycleManager ezer)")]
    [SerializeField]
    private int dailyCafeCustomers = 12;

    [Header("Sokaktan Geçen NPC İhtimali")]
    [Range(0f, 1f)]
    [SerializeField]
    private float streetPedestrianChance = 0.40f;

    [Header("Aynı Prefabın Tekrar Spawn Olmama Ayarı")]
    [Tooltip("Aynı prefab tekrar spawn olabilmeden önce en az bu kadar FARKLI prefab spawn olmalı.")]
    [SerializeField]
    private int prefabCooldownCount = 5;

    private int activeNPCCount = 0;

    // SADECE GERÇEKTEN KUYRUĞA GİREN
    // MÜŞTERİLER BURADA SAYILIR.
    private int cafeCustomersSpawnedToday = 0;

    private float spawnTimer;

    // Gün başlayana (DayCycleManager.StartDay -> StartNewDay)
    // kadar spawn kapalı kalır.
    private bool spawningEnabled = false;

    private readonly List<GameObject> recentlySpawnedPrefabs =
        new List<GameObject>();

    public int ActiveNPCCount =>
        activeNPCCount;

    public int CafeCustomersSpawnedToday =>
        cafeCustomersSpawnedToday;

    public int DailyCafeCustomers =>
        dailyCafeCustomers;

    private void Start()
    {
        SetNextSpawnTime();
    }

    private void Update()
    {
        if (!spawningEnabled)
            return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            TrySpawnNPC();

            SetNextSpawnTime();
        }
    }

    private void SetNextSpawnTime()
    {
        spawnTimer =
            Random.Range(
                minSpawnInterval,
                maxSpawnInterval
            );
    }

    private GameObject SelectRandomPrefab()
    {
        int effectiveCooldown =
            Mathf.Clamp(
                prefabCooldownCount,
                0,
                Mathf.Max(0, npcPrefabs.Count - 1)
            );

        List<GameObject> candidates =
            new List<GameObject>();

        foreach (GameObject prefab in npcPrefabs)
        {
            if (!recentlySpawnedPrefabs.Contains(prefab))
            {
                candidates.Add(prefab);
            }
        }

        if (candidates.Count == 0)
        {
            candidates = new List<GameObject>(npcPrefabs);
        }

        GameObject selected =
            candidates[
                Random.Range(0, candidates.Count)
            ];

        recentlySpawnedPrefabs.Add(selected);

        while (recentlySpawnedPrefabs.Count > effectiveCooldown)
        {
            recentlySpawnedPrefabs.RemoveAt(0);
        }

        return selected;
    }

    private void TrySpawnNPC()
    {
        if (activeNPCCount >= maxActiveNPC)
        {
            return;
        }

        if (npcPrefabs == null ||
            npcPrefabs.Count == 0)
        {
            Debug.LogWarning(
                "NPCSpawner: NPC prefab listesi boş!"
            );

            return;
        }

        if (spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            Debug.LogWarning(
                "NPCSpawner: Spawn point listesi boş!"
            );

            return;
        }

        GameObject selectedPrefab =
            SelectRandomPrefab();

        int spawnIndex =
            Random.Range(
                0,
                spawnPoints.Length
            );

        Transform selectedSpawnPoint =
            spawnPoints[spawnIndex];

        if (selectedSpawnPoint == null)
        {
            Debug.LogWarning(
                "NPCSpawner: Spawn point null!"
            );

            return;
        }

        bool dailyCustomerLimitReached =
            cafeCustomersSpawnedToday >=
            dailyCafeCustomers;

        bool isCafeCustomerCandidate;

        if (dailyCustomerLimitReached)
        {
            // Günlük gerçek müşteri hedefi doldu,
            // bundan sonra sadece sokak NPC'leri.
            isCafeCustomerCandidate = false;
        }
        else
        {
            isCafeCustomerCandidate =
                Random.value >= streetPedestrianChance;
        }

        GameObject spawnedNPC =
            Instantiate(
                selectedPrefab,
                selectedSpawnPoint.position,
                selectedSpawnPoint.rotation
            );

        activeNPCCount++;

        bool fromSpawnPoint1 =
            spawnIndex == 0;

        NPCController npcController =
            spawnedNPC.GetComponent<NPCController>();

        if (npcController != null)
        {
            npcController.InitializeSpawn(
                isCafeCustomerCandidate,
                fromSpawnPoint1,
                this
            );
        }
        else
        {
            Debug.LogError(
                $"{spawnedNPC.name}: " +
                "NPCController bulunamadı!"
            );
        }

        NPCSpawnedTracker tracker =
            spawnedNPC.GetComponent<NPCSpawnedTracker>();

        if (tracker == null)
        {
            tracker =
                spawnedNPC.AddComponent<NPCSpawnedTracker>();
        }

        tracker.Initialize(
            this
        );

        string spawnType =
            isCafeCustomerCandidate
                ? "MÜŞTERİ ADAYI"
                : "SOKAK NPC";

        Debug.Log(
            $"NPC SPAWN: {spawnedNPC.name} | " +
            $"{spawnType} | " +
            $"Spawn: {selectedSpawnPoint.name} | " +
            $"Aktif: {activeNPCCount}/{maxActiveNPC} | " +
            $"Gerçek müşteri: " +
            $"{cafeCustomersSpawnedToday}/{dailyCafeCustomers}"
        );
    }

    public bool RegisterCafeCustomer()
    {
        if (cafeCustomersSpawnedToday >=
            dailyCafeCustomers)
        {
            return false;
        }

        cafeCustomersSpawnedToday++;

        Debug.Log(
            $"GERÇEK KAFE MÜŞTERİSİ: " +
            $"{cafeCustomersSpawnedToday}/" +
            $"{dailyCafeCustomers}"
        );

        return true;
    }

    public void NotifyNPCDestroyed()
    {
        activeNPCCount--;

        if (activeNPCCount < 0)
        {
            activeNPCCount = 0;
        }

        Debug.Log(
            $"NPC yok oldu. " +
            $"Aktif NPC: " +
            $"{activeNPCCount}/{maxActiveNPC}"
        );
    }

    // =========================================================
    // YENİ GÜN (SADECE MÜŞTERİ SAYISI)
    // =========================================================

    public void StartNewDay(
        int newDailyCafeCustomerCount)
    {
        dailyCafeCustomers =
            newDailyCafeCustomerCount;

        cafeCustomersSpawnedToday =
            0;

        spawningEnabled =
            true;

        SetNextSpawnTime();

        Debug.Log(
            $"Yeni gün başladı. " +
            $"Günlük gerçek müşteri hedefi: " +
            $"{dailyCafeCustomers}"
        );
    }

    // =========================================================
    // YENİ GÜN (MÜŞTERİ SAYISI + SPAWN ARALIĞI)
    // =========================================================

    public void StartNewDay(
        int newDailyCafeCustomerCount,
        float newMinSpawnInterval,
        float newMaxSpawnInterval)
    {
        minSpawnInterval =
            Mathf.Max(1f, newMinSpawnInterval);

        maxSpawnInterval =
            Mathf.Max(minSpawnInterval, newMaxSpawnInterval);

        StartNewDay(newDailyCafeCustomerCount);
    }

    public void StopSpawning()
    {
        spawningEnabled = false;

        Debug.Log(
            "NPC spawn sistemi durduruldu."
        );
    }

    public void ResumeSpawning()
    {
        spawningEnabled = true;

        SetNextSpawnTime();

        Debug.Log(
            "NPC spawn sistemi tekrar başlatıldı."
        );
    }
}