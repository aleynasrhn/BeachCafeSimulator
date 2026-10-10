[System.Serializable]
public class DayResult
{
    public int dayNumber;
    public string workHoursText;

    public float totalGrossRevenue;
    public float totalPenalty;
    public float profit;

    public int totalOrdersPlaced;
    public int successfulOrders;
    public int failedOrders;

    public int customersArrived;
    public int satisfiedCustomers;
    public int unsatisfiedCustomers;

    public int starRating;

    // Yıldızın nasıl çıktığını görmek için (0-1 arası puanlar).
    public float accuracyScore;
    public float satisfactionScore;
    public float speedScore;
    public float cleanlinessScore;
    public float finalScore;
}