using System;
using System.Collections.Generic;
using UnityEngine;

public class DayCycleManager : MonoBehaviour
{
    public static DayCycleManager Instance { get; private set; }

    public enum DayPhase
    {
        NotStarted,
        Open,
        NoMoreCustomers,
        Ended
    }

    // =========================================================
    // GÜNLÜK ZORLUK TABLOSU
    // =========================================================

    [Serializable]
    public class DayDifficulty
    {
        public int day = 1;

        [Tooltip("Bu gün kuyruğa girecek toplam gerçek müşteri sayısı.")]
        public int customers = 6;

        [Tooltip("İki NPC spawn'ı arasındaki en kısa süre (sn).")]
        public float minSpawnInterval = 40f;

        [Tooltip("İki NPC spawn'ı arasındaki en uzun süre (sn).")]
        public float maxSpawnInterval = 65f;
    }

    [Header("Çalışma Saatleri")]
    [SerializeField] private int openHour = 9;
    [SerializeField] private int closeHour = 20;

    [Header("Zaman Akış Hızı")]
    [Tooltip("1 oyun-içi saatin gerçek dünyada kaç saniye sürdüğü.")]
    [SerializeField] private float secondsPerGameHour = 60f;

    [Header("Zorluk Eğrisi (Gün Bazlı)")]
    [Tooltip("Gün sırasına göre doldur. Listede olmayan bir gün için, ondan önceki en son girdi kullanılır.")]
    [SerializeField]
    private List<DayDifficulty> difficultyByDay = new List<DayDifficulty>
    {
        new DayDifficulty { day = 1, customers = 6,  minSpawnInterval = 40f, maxSpawnInterval = 65f },
        new DayDifficulty { day = 2, customers = 8,  minSpawnInterval = 30f, maxSpawnInterval = 50f },
        new DayDifficulty { day = 3, customers = 10, minSpawnInterval = 25f, maxSpawnInterval = 40f },
        new DayDifficulty { day = 4, customers = 12, minSpawnInterval = 20f, maxSpawnInterval = 35f },
        new DayDifficulty { day = 5, customers = 14, minSpawnInterval = 18f, maxSpawnInterval = 30f },
        new DayDifficulty { day = 6, customers = 16, minSpawnInterval = 15f, maxSpawnInterval = 27f },
        new DayDifficulty { day = 7, customers = 18, minSpawnInterval = 14f, maxSpawnInterval = 24f },
    };

    [Header("Tablonun Ötesindeki Günler")]
    [SerializeField] private int extraCustomersPerDay = 2;
    [SerializeField] private int maxCustomersPerDay = 30;
    [SerializeField] private float minAllowedSpawnInterval = 8f;

    [Header("Spawn Aralığı Ölçekleme")]
    [Tooltip("Tablodaki spawn aralıkları bu gün hızına göre hazırlandı. Seconds Per Game Hour'u test için küçültünce aralıklar otomatik orantılı kısalır.")]
    [SerializeField] private float referenceSecondsPerGameHour = 60f;

    [Header("Referanslar")]
    [SerializeField] private NPCSpawner npcSpawner;

    private float currentHour;
    private DayPhase currentPhase = DayPhase.NotStarted;

    public DayPhase CurrentPhase => currentPhase;
    public float CurrentHour => currentHour;

    public event Action OnDayStarted;
    public event Action OnNoMoreCustomers;
    public event Action<DayResult> OnDayEnded;
    public event Action OnCannotEndDay;

    // Özet ekranındaki "Devam Et"e basılınca, yeni günün başlatılmaya
    // HAZIR olduğunu bildirir. DayPhaseUI dinleyip "Günü Başlat (Space)"
    // yazısını tekrar gösterir.
    public event Action OnReadyForNextDay;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        currentHour = openHour;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleSpacePressed();
        }

        if (currentPhase == DayPhase.Open)
        {
            AdvanceTime();
        }
    }

    private void HandleSpacePressed()
    {
        switch (currentPhase)
        {
            case DayPhase.NotStarted:
                StartDay();
                break;

            case DayPhase.NoMoreCustomers:
                TryEndDay();
                break;
        }
    }

    private void AdvanceTime()
    {
        currentHour +=
            Time.deltaTime / Mathf.Max(0.01f, secondsPerGameHour);

        if (currentHour >= closeHour)
        {
            currentHour = closeHour;

            currentPhase = DayPhase.NoMoreCustomers;

            if (npcSpawner != null)
            {
                npcSpawner.StopSpawning();
            }

            OnNoMoreCustomers?.Invoke();

            Debug.Log("Kafe kapandı, yeni müşteri gelmeyecek.");
        }
    }

    // =========================================================
    // GÜNÜ BAŞLAT (Space - NotStarted'tan)
    // =========================================================

    public void StartDay()
    {
        if (currentPhase != DayPhase.NotStarted)
            return;

        currentHour = openHour;
        currentPhase = DayPhase.Open;

        DayStatsTracker.Instance?.ResetForNewDay();

        int dayNumber =
            DayManager.Instance != null
                ? DayManager.Instance.CurrentDay
                : 1;

        if (npcSpawner != null)
        {
            DayDifficulty difficulty =
                GetDifficultyForDay(dayNumber);

            if (difficulty != null)
            {
                float scale =
                    secondsPerGameHour /
                    Mathf.Max(0.01f, referenceSecondsPerGameHour);

                float minInterval =
                    Mathf.Max(2f, difficulty.minSpawnInterval * scale);

                float maxInterval =
                    Mathf.Max(minInterval, difficulty.maxSpawnInterval * scale);

                npcSpawner.StartNewDay(
                    difficulty.customers,
                    minInterval,
                    maxInterval
                );

                Debug.Log(
                    $"Gün {dayNumber} zorluğu: " +
                    $"{difficulty.customers} müşteri | " +
                    $"spawn {minInterval:0}-{maxInterval:0} sn"
                );
            }
            else
            {
                // Tablo boşsa spawner'ın kendi Inspector değerleri kullanılır.
                npcSpawner.StartNewDay(npcSpawner.DailyCafeCustomers);
            }
        }

        OnDayStarted?.Invoke();

        Debug.Log("Gün başladı. Kafe açık.");
    }

    // =========================================================
    // O GÜNÜN ZORLUĞUNU BUL
    // =========================================================

    private DayDifficulty GetDifficultyForDay(int day)
    {
        if (difficultyByDay == null || difficultyByDay.Count == 0)
            return null;

        DayDifficulty best = null;
        DayDifficulty last = null;

        foreach (DayDifficulty entry in difficultyByDay)
        {
            if (entry == null)
                continue;

            if (last == null || entry.day > last.day)
            {
                last = entry;
            }

            if (entry.day <= day &&
                (best == null || entry.day > best.day))
            {
                best = entry;
            }
        }

        if (last == null)
            return null;

        // Tablodaki ilk günden bile önceyse ilk girdiyi kullan.
        if (best == null)
        {
            best = difficultyByDay[0];
        }

        // Tablonun içindeki günler: bulunan girdi aynen kullanılır.
        if (day <= last.day)
            return best;

        // Tablonun ötesi: müşteri sayısı artar, aralıklar orantılı kısalır.
        int daysBeyond = day - last.day;

        int customers =
            Mathf.Min(
                last.customers + daysBeyond * extraCustomersPerDay,
                Mathf.Max(maxCustomersPerDay, last.customers)
            );

        float ratio =
            (float)last.customers / Mathf.Max(1, customers);

        float minInterval =
            Mathf.Max(minAllowedSpawnInterval, last.minSpawnInterval * ratio);

        float maxInterval =
            Mathf.Max(minInterval + 1f, last.maxSpawnInterval * ratio);

        return new DayDifficulty
        {
            day = day,
            customers = customers,
            minSpawnInterval = minInterval,
            maxSpawnInterval = maxInterval
        };
    }

    // =========================================================
    // GÜNÜ BİTİREBİLİR Mİ?
    // =========================================================

    public bool CanEndDay()
    {
        return NPCController.ActiveCafeCustomerCount <= 0;
    }

    public void TryEndDay()
    {
        if (currentPhase != DayPhase.NoMoreCustomers)
            return;

        if (!CanEndDay())
        {
            OnCannotEndDay?.Invoke();

            Debug.Log(
                "Gün bitirilemiyor: hâlâ aktif müşteri var " +
                $"({NPCController.ActiveCafeCustomerCount})."
            );

            return;
        }

        EndDay();
    }

    private void EndDay()
    {
        currentPhase = DayPhase.Ended;

        int dayNumber =
            DayManager.Instance != null
                ? DayManager.Instance.CurrentDay
                : 1;

        string workHoursText =
            $"{FormatHour(openHour)} - {FormatHour(closeHour)}";

        DayResult result =
            DayStatsTracker.Instance != null
                ? DayStatsTracker.Instance.BuildResult(dayNumber, workHoursText)
                : new DayResult
                {
                    dayNumber = dayNumber,
                    workHoursText = workHoursText
                };

        OnDayEnded?.Invoke(result);

        Debug.Log($"Gün {dayNumber} sona erdi.");
    }

    // =========================================================
    // ÖZET EKRANINDAKİ BUTON BUNU ÇAĞIRIR
    // =========================================================

    public void PrepareNextDay()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.NextDay();
        }

        currentPhase = DayPhase.NotStarted;
        currentHour = openHour;

        OnReadyForNextDay?.Invoke();

        Debug.Log("Yeni gün hazır, Space ile başlatılabilir.");
    }

    // =========================================================
    // SAAT YAZISI
    // =========================================================

    public string GetTimeString()
    {
        return FormatHour(currentHour);
    }

    private string FormatHour(float hour)
    {
        int h = Mathf.FloorToInt(hour);
        int m = Mathf.FloorToInt((hour - h) * 60f);

        return $"{h:00}:{m:00}";
    }
}