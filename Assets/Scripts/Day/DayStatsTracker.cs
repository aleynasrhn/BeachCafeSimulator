using UnityEngine;

/// <summary>
/// Gün boyunca gelir, sipariş, müşteri ve servis hızı istatistiklerini
/// toplar. Gün bitince DayCycleManager, BuildResult() ile özet ekranı
/// için bir DayResult üretir. Yıldız puanı; sipariş doğruluğu, müşteri
/// memnuniyeti, servis hızı ve temizliğin ağırlıklı ortalamasıdır.
/// </summary>
public class DayStatsTracker : MonoBehaviour
{
    public static DayStatsTracker Instance { get; private set; }

    [Header("Yıldız Puanı Ağırlıkları")]
    [SerializeField] private float accuracyWeight = 0.30f;
    [SerializeField] private float satisfactionWeight = 0.20f;
    [SerializeField] private float speedWeight = 0.25f;
    [SerializeField] private float cleanlinessWeight = 0.25f;

    [Header("Yıldız Eşikleri (Toplam Puan 0-1)")]
    [SerializeField] private float fiveStarThreshold = 0.90f;
    [SerializeField] private float fourStarThreshold = 0.75f;
    [SerializeField] private float threeStarThreshold = 0.55f;
    [SerializeField] private float twoStarThreshold = 0.30f;

    [Header("Temizlik")]
    [Tooltip("Kafe zemininde bu kadar pislik varsa zemin temizlik puanı 0 olur.")]
    [SerializeField] private int floorDirtyCountForZeroScore = 5;

    private int totalOrdersPlaced;
    private float totalGrossRevenue;

    private int successfulOrders;
    private int failedOrders;
    private float totalPenalty;

    private int customersArrived;
    private int satisfiedCustomers;
    private int unsatisfiedCustomers;

    private float speedScoreSum;
    private int servedOrderCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // =========================================================
    // SIFIRLA (HER GÜN BAŞINDA ÇAĞRILIR)
    // =========================================================

    public void ResetForNewDay()
    {
        totalOrdersPlaced = 0;
        totalGrossRevenue = 0f;

        successfulOrders = 0;
        failedOrders = 0;
        totalPenalty = 0f;

        customersArrived = 0;
        satisfiedCustomers = 0;
        unsatisfiedCustomers = 0;

        speedScoreSum = 0f;
        servedOrderCount = 0;
    }

    // =========================================================
    // KAYIT NOKTALARI
    // =========================================================

    // OrderScreenUI: ödeme başarıyla alındığı an çağrılır.
    public void RecordOrderPlaced(float orderTotal)
    {
        totalOrdersPlaced++;
        totalGrossRevenue += orderTotal;
    }

    // NPCController: sipariş doğru teslim edildiğinde ya da
    // yanlış/zaman aşımı olduğunda çağrılır.
    public void RecordOrderResult(bool wasCorrect, float penalty)
    {
        if (wasCorrect)
        {
            successfulOrders++;
        }
        else
        {
            failedOrders++;
            totalPenalty += penalty;
        }
    }

    // NPCController: DOĞRU servis edilen siparişte çağrılır.
    // timeRatio: 0 = anında servis, 1 = süre dolmak üzere.
    // Oran 0.5 ve altıysa tam puan, 1.0'da sıfır puan.
    public void RecordServiceSpeed(float timeRatio)
    {
        float score =
            Mathf.Clamp01((1f - timeRatio) / 0.5f);

        speedScoreSum += score;
        servedOrderCount++;
    }

    // NPCController: NPC gerçekten kuyruğa girdiğinde çağrılır.
    public void RecordCustomerArrived()
    {
        customersArrived++;
    }

    // NPCController: müşteri masadan kalkarken çağrılır.
    public void RecordCustomerLeft(bool wasSatisfied)
    {
        if (wasSatisfied)
            satisfiedCustomers++;
        else
            unsatisfiedCustomers++;
    }

    // =========================================================
    // GÜN SONU ÖZETİ
    // =========================================================

    public DayResult BuildResult(int dayNumber, string workHoursText)
    {
        float profit = totalGrossRevenue - totalPenalty;

        float accuracy =
            totalOrdersPlaced > 0
                ? (float)successfulOrders / totalOrdersPlaced
                : 0f;

        float satisfaction =
            customersArrived > 0
                ? (float)satisfiedCustomers / customersArrived
                : 0f;

        float speed =
            servedOrderCount > 0
                ? speedScoreSum / servedOrderCount
                : 0f;

        float cleanliness =
            ComputeCleanlinessScore();

        bool hadActivity =
            totalOrdersPlaced > 0 || customersArrived > 0;

        float weightSum =
            accuracyWeight +
            satisfactionWeight +
            speedWeight +
            cleanlinessWeight;

        float finalScore =
            weightSum > 0.0001f
                ? (accuracy * accuracyWeight +
                   satisfaction * satisfactionWeight +
                   speed * speedWeight +
                   cleanliness * cleanlinessWeight) / weightSum
                : 0f;

        int stars =
            hadActivity
                ? StarsFromScore(finalScore)
                : 0;

        Debug.Log(
            $"GÜN PUANI | Doğruluk: {accuracy:0.00} | " +
            $"Memnuniyet: {satisfaction:0.00} | " +
            $"Hız: {speed:0.00} | Temizlik: {cleanliness:0.00} | " +
            $"Toplam: {finalScore:0.00} → {stars} yıldız"
        );

        return new DayResult
        {
            dayNumber = dayNumber,
            workHoursText = workHoursText,

            totalGrossRevenue = totalGrossRevenue,
            totalPenalty = totalPenalty,
            profit = profit,

            totalOrdersPlaced = totalOrdersPlaced,
            successfulOrders = successfulOrders,
            failedOrders = failedOrders,

            customersArrived = customersArrived,
            satisfiedCustomers = satisfiedCustomers,
            unsatisfiedCustomers = unsatisfiedCustomers,

            starRating = stars,

            accuracyScore = accuracy,
            satisfactionScore = satisfaction,
            speedScore = speed,
            cleanlinessScore = cleanliness,
            finalScore = finalScore
        };
    }

    // =========================================================
    // TEMİZLİK PUANI (GÜN BİTERKEN ANLIK DURUM)
    // =========================================================

    private float ComputeCleanlinessScore()
    {
        float floorScore = 1f;

        if (CafeFloorLitterZone.Instance != null)
        {
            int dirtyCount =
                CafeFloorLitterZone.Instance.ActiveCount;

            floorScore =
                1f - Mathf.Clamp01(
                    (float)dirtyCount /
                    Mathf.Max(1, floorDirtyCountForZeroScore)
                );
        }

        float toiletScore = 1f;

        if (ToiletManager.Instance != null)
        {
            // 2 kabin + 1 ortak alan = 3 birim
            int units =
                ToiletManager.Instance.StallCount + 1;

            int dirtyUnits =
                ToiletManager.Instance.DirtyStallCount +
                (ToiletManager.Instance.IsCommonAreaDirty ? 1 : 0);

            toiletScore =
                1f - (float)dirtyUnits / Mathf.Max(1, units);
        }

        return (floorScore + toiletScore) * 0.5f;
    }

    // =========================================================
    // PUANDAN YILDIZA
    // =========================================================

    private int StarsFromScore(float score)
    {
        if (score >= fiveStarThreshold) return 5;
        if (score >= fourStarThreshold) return 4;
        if (score >= threeStarThreshold) return 3;
        if (score >= twoStarThreshold) return 2;

        return 1;
    }
}