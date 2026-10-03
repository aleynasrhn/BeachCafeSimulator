using UnityEngine;

/// <summary>
/// Gün boyunca gelir, sipariş ve müşteri istatistiklerini toplar.
/// Diğer scriptler (NPCController, OrderScreenUI) ilgili anlarda
/// bu scripti çağırır. Gün bitince DayCycleManager, BuildResult()
/// ile özet ekranı için bir DayResult üretir.
/// </summary>
public class DayStatsTracker : MonoBehaviour
{
    public static DayStatsTracker Instance { get; private set; }

    private int totalOrdersPlaced;
    private float totalGrossRevenue;

    private int successfulOrders;
    private int failedOrders;
    private float totalPenalty;

    private int customersArrived;
    private int satisfiedCustomers;
    private int unsatisfiedCustomers;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
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
    }

    // =========================================================
    // KAYIT NOKTALARI
    // =========================================================

    // OrderScreenUI: ödeme başarıyla alındığı an (nakit ya da
    // kart) çağrılır. "total" o siparişin tam tutarı.
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

            starRating = CalculateStarRating()
        };
    }

    // =========================================================
    // YILDIZ HESABI
    // =========================================================
    //
    // %60 sipariş başarı oranı + %40 müşteri memnuniyet oranı.
    // Eşikleri beğenmezsen buradan değiştirebilirsin.
    //
    // =========================================================

    private int CalculateStarRating()
    {
        if (totalOrdersPlaced == 0 && customersArrived == 0)
            return 0;

        float orderSuccessRate =
            totalOrdersPlaced > 0
                ? (float)successfulOrders / totalOrdersPlaced
                : 0f;

        float customerSatisfactionRate =
            customersArrived > 0
                ? (float)satisfiedCustomers / customersArrived
                : 0f;

        float combinedScore =
            orderSuccessRate * 0.6f +
            customerSatisfactionRate * 0.4f;

        if (combinedScore >= 0.95f) return 5;
        if (combinedScore >= 0.80f) return 4;
        if (combinedScore >= 0.60f) return 3;
        if (combinedScore >= 0.35f) return 2;

        return 1;
    }
}