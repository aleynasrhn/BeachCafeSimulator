using System;
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

    [Header("Çalışma Saatleri")]
    [SerializeField] private int openHour = 9;
    [SerializeField] private int closeHour = 20;

    [Header("Zaman Akış Hızı")]
    [Tooltip("1 oyun-içi saatin gerçek dünyada kaç saniye sürdüğü.")]
    [SerializeField] private float secondsPerGameHour = 60f;

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
        Debug.Log(
            $"SPACE BASILDI | Phase: {currentPhase} | " +
            $"Aktif Müşteri: {NPCController.ActiveCafeCustomerCount}"
        );

        switch (currentPhase)
        {
            case DayPhase.NotStarted:

                Debug.Log("SPACE → Gün başlatılıyor.");

                StartDay();

                break;

            case DayPhase.NoMoreCustomers:

                Debug.Log(
                    "SPACE → Gün bitirme deneniyor. " +
                    $"Aktif müşteri: {NPCController.ActiveCafeCustomerCount}"
                );

                TryEndDay();

                break;

            case DayPhase.Open:

                Debug.Log(
                    "SPACE → Kafe hâlâ açık, gün bitirilemez."
                );

                break;

            case DayPhase.Ended:

                Debug.Log(
                    "SPACE → Gün zaten bitmiş."
                );

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

        if (npcSpawner != null)
        {
            npcSpawner.StartNewDay(npcSpawner.DailyCafeCustomers);
        }

        OnDayStarted?.Invoke();

        Debug.Log("Gün başladı. Kafe açık.");
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