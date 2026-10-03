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
}