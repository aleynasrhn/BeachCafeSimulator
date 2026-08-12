using UnityEngine;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Kasa ekranındaki (ComputerInteraction/RegisterInteraction'ın açtığı Canvas'taki)
/// tüm buton mantığını yönetir: boyut + kahve + ekstra seçimi, sepet, doğrulama,
/// ve onaylanan siparişin mevcut OrderUI (sağ panel) sistemine eklenmesi.
/// </summary>
public class OrderScreenUI : MonoBehaviour
{
    [Header("Müşteri Talebi (üstteki yazı)")]
    [SerializeField] private TMP_Text customerRequestText;

    [Header("Doğrulama Mesajı (altta küçük uyarı)")]
    [SerializeField] private TMP_Text validationText;

    [Header("Boyut Butonları (3 tane)")]
    [SerializeField] private SizeButtonUI[] sizeButtons;

    [Header("Ana Kahve Butonları (4 tane)")]
    [SerializeField] private CoffeeButtonUI[] coffeeButtons;

    [Header("Ekstra Butonları")]
    [SerializeField] private ExtraButtonUI[] extraButtons;

    [Header("Sepet")]
    [SerializeField] private TMP_Text basketText;
    [SerializeField] private TMP_Text totalText;

    [Header("Ödeme")]
    [SerializeField] private TMP_Text paymentMethodText;

    [Header("Sipariş Ayarları")]
    [SerializeField] private float orderTimeLimit = 90f;

    private CupSize? selectedSize = null;
    private CoffeeButtonUI selectedCoffee = null;
    private readonly List<ExtraButtonUI> selectedExtras = new List<ExtraButtonUI>();
    private string selectedPaymentMethod = "";

    private Order currentTargetOrder;

    private static readonly string[] extraNamePool = { "Ekstra Espresso", "Tarçın", "Çikolata Şurubu", "Karamel Şurup", "Vanilya Şurubu" };
    private static readonly string[] paymentPool = { "Nakit Ödeme", "Kart Ödeme" };

    private void OnEnable()
    {
        ResetBasket();
        GenerateNewCustomerRequest();
    }

    // ---------- Müşteri Talebi ----------

    private void GenerateNewCustomerRequest()
    {
        currentTargetOrder = new Order
        {
            coffeeType = (CoffeeType)Random.Range(0, System.Enum.GetValues(typeof(CoffeeType)).Length),
            size = (CupSize)Random.Range(0, System.Enum.GetValues(typeof(CupSize)).Length),
            reward = 0,
            timeLimit = orderTimeLimit,
            preferredPaymentMethod = paymentPool[Random.Range(0, paymentPool.Length)]
        };

        // %50 ihtimalle bir ekstra da istesin
        if (Random.value > 0.5f)
            currentTargetOrder.requestedExtras.Add(extraNamePool[Random.Range(0, extraNamePool.Length)]);

        UpdateCustomerRequestText();
    }

    private void UpdateCustomerRequestText()
    {
        if (customerRequestText == null || currentTargetOrder == null) return;

        string extrasPart = currentTargetOrder.requestedExtras.Count > 0
            ? ", " + string.Join(", ", currentTargetOrder.requestedExtras)
            : "";

        customerRequestText.text =
            $"Müşterinin İsteği: {SizeToTurkish(currentTargetOrder.size)} {TypeToTurkish(currentTargetOrder.coffeeType)}{extrasPart} / {currentTargetOrder.preferredPaymentMethod}";
    }

    // ---------- Seçimler ----------

    /// <summary>Boyut butonlarına (Büyük/Orta/Small) OnClick ile bağlanır.</summary>
    public void SelectSize(SizeButtonUI button)
    {
        selectedSize = button.Size;
        ClearValidationMessage();
        UpdateBasketDisplay();
    }

    /// <summary>Ana kahve butonlarına OnClick ile bağlanır.</summary>
    public void SelectCoffee(CoffeeButtonUI button)
    {
        selectedCoffee = button;
        ClearValidationMessage();
        UpdateBasketDisplay();
    }

    /// <summary>Ekstra butonlarına OnClick ile bağlanır.</summary>
    public void ToggleExtra(ExtraButtonUI button)
    {
        if (button.IsLocked) return;

        if (selectedExtras.Contains(button))
            selectedExtras.Remove(button);
        else
            selectedExtras.Add(button);

        UpdateBasketDisplay();
    }

    /// <summary>Nakit Al / Kredi Kartı butonlarına OnClick ile bağlanır, parametre olarak string yaz.</summary>
    public void SelectPaymentMethod(string method)
    {
        selectedPaymentMethod = method;
        if (paymentMethodText != null) paymentMethodText.text = method;
    }

    // ---------- Sepet ----------

    private void UpdateBasketDisplay()
    {
        float total = 0f;
        string lines = "";

        // Kahve SADECE hem tür hem boyut seçiliyse sepette görünür
        if (selectedCoffee != null && selectedSize.HasValue)
        {
            float price = selectedCoffee.GetPrice(selectedSize.Value);
            lines += $"{SizeToTurkish(selectedSize.Value)} {selectedCoffee.CoffeeName} {price:0.00}$\n";
            total += price;
        }

        foreach (var extra in selectedExtras)
        {
            lines += $"{extra.ExtraName} {extra.Price:0.00}$\n";
            total += extra.Price;
        }

        if (basketText != null) basketText.text = lines;
        if (totalText != null) totalText.text = $"Toplam: {total:0.00}$";
    }

    /// <summary>"Sepeti Sıfırla" butonuna bağlanır.</summary>
    public void ResetBasket()
    {
        selectedCoffee = null;
        selectedSize = null;
        selectedExtras.Clear();
        selectedPaymentMethod = "";

        if (paymentMethodText != null) paymentMethodText.text = "";

        UpdateBasketDisplay();
        ClearValidationMessage();
    }

    // ---------- Onaylama ----------

    /// <summary>"Siparişi Onayla" butonuna bağlanır.</summary>
    public void ConfirmOrder()
    {
        if (selectedCoffee == null)
        {
            ShowValidationMessage("Lütfen bir kahve seçin.");
            return;
        }

        if (!selectedSize.HasValue)
        {
            ShowValidationMessage("Lütfen boyut seçin.");
            return;
        }

        float total = selectedCoffee.GetPrice(selectedSize.Value);
        foreach (var extra in selectedExtras) total += extra.Price;

        Order finalOrder = new Order
        {
            coffeeType = selectedCoffee.CoffeeType,
            size = selectedSize.Value,
            reward = Mathf.RoundToInt(total),
            timeLimit = orderTimeLimit
        };

        // Sağdaki mevcut sipariş paneline ekle
        if (OrderUI.Instance != null)
            OrderUI.Instance.AddOrder(finalOrder);

        string summary = $"{SizeToTurkish(selectedSize.Value)} {selectedCoffee.CoffeeName}";
        foreach (var extra in selectedExtras) summary += $" + {extra.ExtraName}";

        Debug.Log($"Sipariş onaylandı: {summary} - {selectedPaymentMethod} - {total:0.00}$");

        ResetBasket();
        GenerateNewCustomerRequest();
    }

    private void ShowValidationMessage(string message)
    {
        if (validationText != null) validationText.text = message;
    }

    private void ClearValidationMessage()
    {
        if (validationText != null) validationText.text = "";
    }

    // ---------- Yardımcılar ----------

    private string TypeToTurkish(CoffeeType type)
    {
        switch (type)
        {
            case CoffeeType.Espresso: return "Espresso";
            case CoffeeType.Latte: return "Latte";
            case CoffeeType.Cappuccino: return "Cappuccino";
            case CoffeeType.Americano: return "Americano";
        }
        return "";
    }

    private string SizeToTurkish(CupSize size)
    {
        switch (size)
        {
            case CupSize.Small: return "Small";
            case CupSize.Medium: return "Orta";
            case CupSize.Large: return "Büyük";
        }
        return "";
    }
}