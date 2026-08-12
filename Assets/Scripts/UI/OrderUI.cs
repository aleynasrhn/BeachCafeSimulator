using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderUI : MonoBehaviour
{
    public static OrderUI Instance;

    [Header("References")]
    public Transform orderContainer;
    public GameObject orderItemPrefab;

    [Header("Order Limit UI")]
    public TMP_Text orderCountText;
    public Image progressFill;

    [Header("States")]
    public GameObject orderState;
    public GameObject emptyState;

    private int activeOrderCount = 0;
    private const int maxOrders = 3;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddOrder(Order order)
    {
        if (activeOrderCount >= maxOrders)
        {
            Debug.Log("Maksimum sipariş sayısına ulaşıldı.");
            return;
        }

        GameObject newItem = Instantiate(orderItemPrefab, orderContainer);

        OrderItemUI itemUI = newItem.GetComponent<OrderItemUI>();
        itemUI.Setup(order);

        activeOrderCount++;

        UpdateUI();
    }

    public void RemoveOrder()
    {
        if (activeOrderCount > 0)
        {
            activeOrderCount--;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        // Alttaki sayaç
        orderCountText.text = "BEKLEYEN SİPARİŞLER: " + activeOrderCount;

        // Bar
        progressFill.fillAmount = (float)activeOrderCount / maxOrders;

        // 0 sipariş = boş ekran
        bool hasOrders = activeOrderCount > 0;

        orderState.SetActive(hasOrders);
        emptyState.SetActive(!hasOrders);
    }
}