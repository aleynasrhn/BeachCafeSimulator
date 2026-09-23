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

    [Header("Spawn Süresi")]
    [SerializeField]
    private float minSpawnInterval = 5f;

    [SerializeField]
    private float maxSpawnInterval = 10f;

    [Header("Günlük Müşteri")]
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

    private bool spawningEnabled = true;

    // =========================================================
    // SON SPAWN EDİLEN PREFAB GEÇMİŞİ (TEKRAR ENGELİ İÇİN)
    // =========================================================

    private readonly List<GameObject> recentlySpawnedPrefabs =
        new List<GameObject>();

    // =========================================================
    // PUBLIC BİLGİLER
    // =========================================================

    public int ActiveNPCCount =>
        activeNPCCount;

    public int CafeCustomersSpawnedToday =>
        cafeCustomersSpawnedToday;

    public int DailyCafeCustomers =>
        dailyCafeCustomers;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        SetNextSpawnTime();
    }

    // =========================================================
    // UPDATE
    // =========================================================

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

    // =========================================================
    // SONRAKİ SPAWN SÜRESİ
    // =========================================================

    private void SetNextSpawnTime()
    {
        spawnTimer =
            Random.Range(
                minSpawnInterval,
                maxSpawnInterval
            );
    }

    // =========================================================
    // TEKRAR ETMEYEN RASTGELE PREFAB SEÇ
    // =========================================================
    //
    // Son "prefabCooldownCount" kadar spawn edilen prefab
    // hariç tutulup, kalan uygun prefablardan rastgele biri
    // seçilir. Böylece aynı karakter (örn. Lisa) art arda ya
    // da çok kısa aralıklarla tekrar spawn olamaz.
    //
    // =========================================================

    private GameObject SelectRandomPrefab()
    {
        // Prefab sayısından fazla cooldown istenirse, listeyi
        // tamamen boşaltmamak için sınırla.
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

        // Güvenlik: hiç uygun aday kalmadıysa (olmaması gerekir
        // ama olursa) tüm listeyi kullan.
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

    // =========================================================
    // NPC SPAWN
    // =========================================================

    private void TrySpawnNPC()
    {
        // ---------------------------------------------------------
        // AKTİF NPC LİMİTİ
        // ---------------------------------------------------------

        if (activeNPCCount >= maxActiveNPC)
        {
            return;
        }

        // ---------------------------------------------------------
        // PREFAB KONTROLÜ
        // ---------------------------------------------------------

        if (npcPrefabs == null ||
            npcPrefabs.Count == 0)
        {
            Debug.LogWarning(
                "NPCSpawner: NPC prefab listesi boş!"
            );

            return;
        }

        // ---------------------------------------------------------
        // SPAWN POINT KONTROLÜ
        // ---------------------------------------------------------

        if (spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            Debug.LogWarning(
                "NPCSpawner: Spawn point listesi boş!"
            );

            return;
        }

        // ---------------------------------------------------------
        // TEKRAR ETMEYEN RASTGELE NPC PREFAB
        // ---------------------------------------------------------

        GameObject selectedPrefab =
            SelectRandomPrefab();

        // ---------------------------------------------------------
        // RASTGELE SPAWN POINT
        // ---------------------------------------------------------

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

        // ---------------------------------------------------------
        // BU NPC MÜŞTERİ ADAYI MI?
        // ---------------------------------------------------------
        //
        // Günlük gerçek müşteri sayısı henüz dolmadıysa
        // NPC'nin müşteri olma ihtimali var.
        //
        // Burada henüz sayacı artırmıyoruz!
        // NPC gerçekten queue'ya girince artıracağız.
        //
        // ---------------------------------------------------------

        bool dailyCustomerLimitReached =
            cafeCustomersSpawnedToday >=
            dailyCafeCustomers;

        bool isCafeCustomerCandidate;

        if (dailyCustomerLimitReached)
        {
            // Günlük 12 gerçek müşteri tamamlandı.
            // Bundan sonra sadece sokak NPC'leri.
            isCafeCustomerCandidate = false;
        }
        else
        {
            // Örneğin %40 sokak,
            // %60 müşteri adayı.
            isCafeCustomerCandidate =
                Random.value >= streetPedestrianChance;
        }

        // ---------------------------------------------------------
        // NPC OLUŞTUR
        // ---------------------------------------------------------

        GameObject spawnedNPC =
            Instantiate(
                selectedPrefab,
                selectedSpawnPoint.position,
                selectedSpawnPoint.rotation
            );

        activeNPCCount++;

        // ---------------------------------------------------------
        // SPAWN TARAFI
        // ---------------------------------------------------------

        bool fromSpawnPoint1 =
            spawnIndex == 0;

        // ---------------------------------------------------------
        // NPC CONTROLLER
        // ---------------------------------------------------------

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

        // ---------------------------------------------------------
        // TRACKER
        // ---------------------------------------------------------

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

        // ---------------------------------------------------------
        // DEBUG
        // ---------------------------------------------------------

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

    // =========================================================
    // NPC GERÇEKTEN KUYRUĞA GİRDİ
    // =========================================================

    public bool RegisterCafeCustomer()
    {
        // Günlük limit dolduysa bu NPC müşteri olamaz.
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

    // =========================================================
    // NPC YOK OLDU
    // =========================================================

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
    // YENİ GÜN
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
    // SPAWN'I DURDUR
    // =========================================================

    public void StopSpawning()
    {
        spawningEnabled = false;

        Debug.Log(
            "NPC spawn sistemi durduruldu."
        );
    }

    // =========================================================
    // SPAWN'I TEKRAR BAŞLAT
    // =========================================================

    public void ResumeSpawning()
    {
        spawningEnabled = true;

        SetNextSpawnTime();

        Debug.Log(
            "NPC spawn sistemi tekrar başlatıldı."
        );
    }
}