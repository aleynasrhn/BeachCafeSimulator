using System.Collections.Generic;
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

    private const int maxVisibleOrders = 3;

    // =========================================================
    // BİLET (TICKET) VERİSİ
    // =========================================================
    //
    // Hem ekranda görünen hem sırada bekleyen tüm siparişler
    // burada tutulur. Süre, görünür olsun olmasın her frame'de
    // azalır — yani sırada bekleyen bir sipariş de arka planda
    // saymaya devam eder.
    //
    // =========================================================

    private class TicketData
    {
        public Order order;
        public NPCController ownerNPC;
        public float remainingTime;
        public OrderItemUI displayedUI;
    }

    private readonly List<TicketData> allTickets =
        new List<TicketData>();


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        Instance = this;
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        UpdateSummaryUI();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        UpdateAllTimers();
    }

    // =========================================================
    // YENİ SİPARİŞ EKLE
    // =========================================================

    public void AddOrder(Order order, NPCController ownerNPC)
    {
        TicketData ticket = new TicketData
        {
            order = order,
            ownerNPC = ownerNPC,
            remainingTime = order.timeLimit,
            displayedUI = null
        };

        allTickets.Add(ticket);

        Debug.Log(
            $"Yeni sipariş bilete eklendi: {order.coffeeType} | " +
            $"Süre: {order.timeLimit:0.0} sn | " +
            $"Toplam bekleyen: {allTickets.Count}"
        );

        TryFillVisibleSlots();

        UpdateSummaryUI();
    }

    // =========================================================
    // BELİRLİ NPC'NİN SİPARİŞİNİ TAMAMLANMIŞ SAY
    // =========================================================
    //
    // Kahve teslim edildiğinde (doğru/yanlış) ya da müşteri
    // süresi dolup kalktığında NPCController bu metodu çağırır.
    // Bilet, görünür olsun olmasın hemen listeden kaldırılır.
    //
    // =========================================================

    public void CompleteOrderForNPC(NPCController npc)
    {
        if (npc == null)
            return;

        TicketData ticket =
            allTickets.Find(t => t.ownerNPC == npc);

        if (ticket == null)
            return;

        RemoveTicket(ticket);
    }

    // =========================================================
    // HER FRAME SÜRELERİ GÜNCELLE
    // =========================================================

    private void UpdateAllTimers()
    {
        for (int i = allTickets.Count - 1; i >= 0; i--)
        {
            TicketData ticket = allTickets[i];

            ticket.remainingTime -= Time.deltaTime;

            if (ticket.displayedUI != null)
            {
                ticket.displayedUI.UpdateTimerDisplay(
                    Mathf.Max(0f, ticket.remainingTime)
                );
            }

            if (ticket.remainingTime <= 0f)
            {
                RemoveTicket(ticket);
            }
        }
    }

    // =========================================================
    // BİLETİ SİL (TESLİM EDİLDİ YA DA SÜRESİ DOLDU)
    // =========================================================

    private void RemoveTicket(TicketData ticket)
    {
        if (!allTickets.Contains(ticket))
            return;

        bool wasVisible =
            ticket.displayedUI != null;

        if (ticket.displayedUI != null)
        {
            Destroy(ticket.displayedUI.gameObject);
            ticket.displayedUI = null;
        }

        allTickets.Remove(ticket);

        if (wasVisible)
        {
            // Görünür bir slot boşaldı, sıradaki siparişi getir.
            TryFillVisibleSlots();
        }

        UpdateSummaryUI();
    }

    // =========================================================
    // BOŞ SLOT VARSA SIRADAKİ SİPARİŞİ GÖSTER
    // =========================================================

    private void TryFillVisibleSlots()
    {
        int visibleCount = 0;

        foreach (TicketData t in allTickets)
        {
            if (t.displayedUI != null)
            {
                visibleCount++;
            }
        }

        foreach (TicketData ticket in allTickets)
        {
            if (visibleCount >= maxVisibleOrders)
                break;

            if (ticket.displayedUI != null)
                continue;

            GameObject newItem =
                Instantiate(orderItemPrefab, orderContainer);

            OrderItemUI itemUI =
                newItem.GetComponent<OrderItemUI>();

            itemUI.Setup(ticket.order);

            itemUI.UpdateTimerDisplay(
                Mathf.Max(0f, ticket.remainingTime)
            );

            ticket.displayedUI = itemUI;

            visibleCount++;
        }
    }

    // =========================================================
    // ÖZET UI (ALTTAKİ SAYAÇ + BAR)
    // =========================================================

    private void UpdateSummaryUI()
    {
        int totalCount = allTickets.Count;

        if (orderCountText != null)
        {
            orderCountText.text =
                "BEKLEYEN SİPARİŞLER: " + totalCount;
        }

        if (progressFill != null)
        {
            progressFill.fillAmount =
                Mathf.Clamp01(
                    (float)totalCount / maxVisibleOrders
                );
        }

        bool hasOrders = totalCount > 0;

        if (orderState != null)
        {
            orderState.SetActive(hasOrders);
        }

        if (emptyState != null)
        {
            emptyState.SetActive(!hasOrders);
        }
    }
}