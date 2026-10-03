using UnityEngine;

public class UnlockManager : MonoBehaviour
{
    public static UnlockManager Instance { get; private set; }


    public static bool InstanceExists =>
        Instance != null;


    [Header("Kahve Butonları")]
    [SerializeField] private CoffeeButtonUI[] coffeeButtons;

    [Header("Ekstra Butonları")]
    [SerializeField] private ExtraButtonUI[] extraButtons;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);

            return;
        }


        Instance = this;


        if (DayManager.Instance == null)
        {
            Debug.LogWarning(
                "UnlockManager: DayManager bulunamadı!"
            );

            return;
        }


        DayManager.Instance.OnDayChanged +=
            UpdateUnlocks;
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (DayManager.Instance == null)
            return;


        UpdateUnlocks(
            DayManager.Instance.CurrentDay
        );
    }


    // =========================================================
    // KİLİTLERİ GÜNCELLE
    // =========================================================

    private void UpdateUnlocks(
        int currentDay)
    {
        // -----------------------------------------------------
        // KAHVELER
        // -----------------------------------------------------

        if (coffeeButtons != null)
        {
            foreach (
                CoffeeButtonUI coffee
                in coffeeButtons)
            {
                if (coffee == null)
                    continue;


                bool unlocked =
                    currentDay >=
                    coffee.UnlockDay;


                coffee.SetUnlocked(
                    unlocked
                );
            }
        }


        // -----------------------------------------------------
        // EKSTRALAR
        // -----------------------------------------------------

        if (extraButtons != null)
        {
            foreach (
                ExtraButtonUI extra
                in extraButtons)
            {
                if (extra == null)
                    continue;


                bool unlocked =
                    currentDay >=
                    extra.UnlockDay;


                extra.SetUnlocked(
                    unlocked
                );
            }
        }


        Debug.Log(
            "Kilitsiz içerikler güncellendi. Gün: " +
            currentDay
        );
    }


    // =========================================================
    // KAHVE AÇIK MI?
    // =========================================================

    public bool IsCoffeeUnlocked(
        CoffeeType coffeeType)
    {
        if (coffeeButtons == null)
            return false;


        foreach (
            CoffeeButtonUI coffee
            in coffeeButtons)
        {
            if (coffee == null)
                continue;


            if (coffee.CoffeeType ==
                coffeeType)
            {
                return coffee.IsUnlocked;
            }
        }


        return false;
    }


    // =========================================================
    // EKSTRA AÇIK MI?
    // =========================================================

    public bool IsExtraUnlocked(
        string extraName)
    {
        if (extraButtons == null)
            return false;


        foreach (
            ExtraButtonUI extra
            in extraButtons)
        {
            if (extra == null)
                continue;


            if (extra.ExtraName ==
                extraName)
            {
                return extra.IsUnlocked;
            }
        }


        return false;
    }


    // =========================================================
    // ON DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayChanged -=
                UpdateUnlocks;
        }


        if (Instance == this)
        {
            Instance = null;
        }
    }
}