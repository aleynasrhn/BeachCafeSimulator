using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance;

    public Order currentOrder;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCurrentOrder(Order order)
    {
        currentOrder = order;

        Debug.Log(
            "OrderManager yeni müşteri siparişini aldı."
        );
    }

    public Order GetCurrentOrder()
    {
        return currentOrder;
    }
}
