using UnityEngine;
using System;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }


    // =========================================================
    // AYARLAR
    // =========================================================

    [Header("Gün Ayarları")]
    [SerializeField] private int startingDay = 1;


    // =========================================================
    // DURUM
    // =========================================================

    private int currentDay;


    public int CurrentDay =>
        currentDay;


    // Gün değiştiğinde diğer sistemler bunu dinleyebilir.
    public event Action<int> OnDayChanged;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        // Singleton
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Sahne değişse bile kalmasını istiyoruz.
        DontDestroyOnLoad(gameObject);


        // Gün başlangıcı
        currentDay =
            Mathf.Max(
                1,
                startingDay
            );
    }


    // =========================================================
    // GÜNÜ BAŞLAT
    // =========================================================

    public void StartDay(int day)
    {
        currentDay =
            Mathf.Max(
                1,
                day
            );


        Debug.Log(
            "Gün başladı: " +
            currentDay
        );


        OnDayChanged?.Invoke(
            currentDay
        );
    }


    // =========================================================
    // SONRAKİ GÜN
    // =========================================================

    public void NextDay()
    {
        currentDay++;


        Debug.Log(
            "Yeni gün başladı: " +
            currentDay
        );


        OnDayChanged?.Invoke(
            currentDay
        );
    }


    // =========================================================
    // BELİRLİ BİR GÜNE GİT
    // =========================================================

    public void SetDay(int day)
    {
        currentDay =
            Mathf.Max(
                1,
                day
            );


        Debug.Log(
            "Gün ayarlandı: " +
            currentDay
        );


        OnDayChanged?.Invoke(
            currentDay
        );
    }


    // =========================================================
    // TEST
    // =========================================================

    [ContextMenu("Test - Sonraki Gün")]
    private void TestNextDay()
    {
        NextDay();
    }


    [ContextMenu("Test - Gün 1")]
    private void TestDay1()
    {
        SetDay(1);
    }


    [ContextMenu("Test - Gün 3")]
    private void TestDay3()
    {
        SetDay(3);
    }


    [ContextMenu("Test - Gün 5")]
    private void TestDay5()
    {
        SetDay(5);
    }


    [ContextMenu("Test - Gün 7")]
    private void TestDay7()
    {
        SetDay(7);
    }
}