using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OrderScreenUI : MonoBehaviour
{
    public static OrderScreenUI Instance;


    [Header("Müşteri Talebi")]
    [SerializeField] private TMP_Text customerRequestText;


    [Header("Doğrulama Mesajı")]
    [SerializeField] private TMP_Text validationText;
    [SerializeField] private GameObject validationBackground;


    [Header("Boyut Butonları")]
    [SerializeField] private SizeButtonUI[] sizeButtons;


    [Header("Espresso Shot Butonları")]
    [SerializeField] private EspressoShotButtonUI[] espressoShotButtons;


    [Header("Ana Kahve Butonları")]
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


    // =========================================================
    // SEÇİMLER
    // =========================================================

    private CupSize? selectedSize = null;

    private CoffeeButtonUI selectedCoffee = null;

    private EspressoShotButtonUI selectedEspressoShot = null;

    private readonly List<ExtraButtonUI> selectedExtras =
        new List<ExtraButtonUI>();

    private string selectedPaymentMethod = "";


    private Order currentTargetOrder;

    public Order CurrentTargetOrder =>
    currentTargetOrder;


    private NPCController currentCustomerNPC;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }


    // =========================================================
    // EKRAN AÇILDIĞINDA
    // =========================================================

    private void OnEnable()
    {
        ResetBasket();
    }


    // =========================================================
    // NPC'DEN SİPARİŞ AL
    // =========================================================

    public void SetCustomerOrder(
    Order order,
    NPCController npc)
    {
        if (order == null)
        {
            Debug.LogWarning(
                "SetCustomerOrder: Order null!"
            );

            return;
        }

        currentTargetOrder =
            order;

        currentCustomerNPC =
            npc;

        // =====================================================
        // MÜŞTERİNİN GERÇEK SİPARİŞ FİYATINI KAYDET
        // =====================================================

        currentTargetOrder.pricePaid =
            CalculateCustomerOrderPrice(
                currentTargetOrder
            );

        Debug.Log(
            $"Müşteri gerçek sipariş fiyatı: " +
            $"{currentTargetOrder.pricePaid:0.00}$"
        );

        ResetBasket();

        UpdateCustomerRequestText();

        Debug.Log(
            "Kasa ekranına NPC siparişi geldi: " +
            order.coffeeType +
            (npc != null
                ? $" ({npc.gameObject.name})"
                : " (NPC referansı yok!)")
        );
    }

    // =========================================================
    // MÜŞTERİNİN GERÇEK SİPARİŞ FİYATINI HESAPLA
    // =========================================================

    public float CalculateCustomerOrderPrice(Order order)
    {
        if (order == null)
            return 0f;

        float total = 0f;

        // ---------------------------------------------------------
        // ESPRESSO
        // ---------------------------------------------------------

        if (order.coffeeType == CoffeeType.Espresso)
        {
            if (order.espressoShot ==
                EspressoShotButtonUI.ShotType.Single)
            {
                total = 2.15f;
            }
            else
            {
                total = 2.75f;
            }
        }

        // ---------------------------------------------------------
        // NORMAL KAHVELER
        // ---------------------------------------------------------

        else
        {
            CoffeeButtonUI coffeeButton =
                System.Array.Find(
                    coffeeButtons,
                    coffee =>
                        coffee != null &&
                        coffee.CoffeeType == order.coffeeType
                );

            if (coffeeButton != null)
            {
                total =
                    coffeeButton.GetPrice(
                        order.size
                    );
            }
            else
            {
                Debug.LogWarning(
                    $"Müşteri kahvesi için CoffeeButtonUI bulunamadı: " +
                    $"{order.coffeeType}"
                );
            }
        }

        // ---------------------------------------------------------
        // EKSTRALAR
        // ---------------------------------------------------------

        foreach (string extraName in order.requestedExtras)
        {
            ExtraButtonUI extraButton =
                System.Array.Find(
                    extraButtons,
                    extra =>
                        extra != null &&
                        extra.ExtraName == extraName
                );

            if (extraButton != null)
            {
                total += extraButton.Price;
            }
            else
            {
                Debug.LogWarning(
                    $"Müşteri ekstrası için ExtraButtonUI bulunamadı: " +
                    $"{extraName}"
                );
            }
        }

        return total;
    }


    // =========================================================
    // MÜŞTERİ TALEBİ YAZISI
    // =========================================================

    private void UpdateCustomerRequestText()
    {
        if (customerRequestText == null ||
            currentTargetOrder == null)
        {
            return;
        }


        string coffeePart;


        if (currentTargetOrder.coffeeType ==
            CoffeeType.Espresso)
        {
            string shotName;


            if (currentTargetOrder.espressoShot ==
                EspressoShotButtonUI.ShotType.Single)
            {
                shotName =
                    "Tek Shot";
            }
            else
            {
                shotName =
                    "Double Shot";
            }


            coffeePart =
                $"Espresso, {shotName}";
        }
        else
        {
            coffeePart =
                $"{SizeToTurkish(currentTargetOrder.size)} " +
                $"{TypeToTurkish(currentTargetOrder.coffeeType)}";
        }


        string extrasPart = "";


        if (currentTargetOrder.requestedExtras != null &&
            currentTargetOrder.requestedExtras.Count > 0)
        {
            extrasPart =
                ", " +
                string.Join(
                    ", ",
                    currentTargetOrder.requestedExtras
                );
        }


        customerRequestText.text =
            $"Müşterinin İsteği: {coffeePart}" +
            $"{extrasPart} / " +
            $"{currentTargetOrder.preferredPaymentMethod}";
    }


    // =========================================================
    // ESPRESSO SEÇİLİ Mİ?
    // =========================================================

    private bool IsEspressoSelected()
    {
        return selectedCoffee != null &&
               selectedCoffee.CoffeeType ==
               CoffeeType.Espresso;
    }


    // =========================================================
    // BOYUT SEÇ
    // =========================================================

    public void SelectSize(
        SizeButtonUI button)
    {
        if (button == null)
            return;


        if (IsEspressoSelected())
        {
            ShowValidationMessage(
                "Espresso her zaman küçük bardakta hazırlanır."
            );

            return;
        }


        if (selectedEspressoShot != null)
        {
            ShowValidationMessage(
                "Espresso seçmeden shot seçemezsiniz."
            );

            return;
        }


        selectedSize =
            button.Size;


        ClearValidationMessage();


        UpdateBasketDisplay();
    }


    // =========================================================
    // KAHVE SEÇ
    // =========================================================

    public void SelectCoffee(
        CoffeeButtonUI button)
    {
        if (button == null)
            return;


        if (!button.IsUnlocked)
        {
            ShowValidationMessage(
                "Bu kahve henüz açılmadı."
            );

            return;
        }


        if (selectedEspressoShot != null &&
            button.CoffeeType !=
            CoffeeType.Espresso)
        {
            ShowValidationMessage(
                "Tek Shot ve Double Shot sadece Espresso için kullanılabilir."
            );

            return;
        }


        selectedCoffee =
            button;


        if (button.CoffeeType ==
            CoffeeType.Espresso)
        {
            selectedSize = null;
        }
        else
        {
            selectedEspressoShot = null;
        }


        ClearValidationMessage();


        UpdateBasketDisplay();
    }


    // =========================================================
    // ESPRESSO SHOT SEÇ
    // =========================================================

    public void SelectEspressoShot(
        EspressoShotButtonUI button)
    {
        if (button == null)
            return;


        if (selectedCoffee == null)
        {
            ShowValidationMessage(
                "Önce Espresso seçin."
            );

            return;
        }


        if (selectedCoffee.CoffeeType !=
            CoffeeType.Espresso)
        {
            ShowValidationMessage(
                "Tek Shot ve Double Shot sadece Espresso için kullanılabilir."
            );

            return;
        }


        selectedEspressoShot =
            button;


        selectedSize = null;


        ClearValidationMessage();


        UpdateBasketDisplay();
    }


    // =========================================================
    // EKSTRA SEÇ / ÇIKAR
    // =========================================================

    public void ToggleExtra(
        ExtraButtonUI button)
    {
        if (button == null)
            return;


        if (button.IsLocked)
            return;


        if (selectedCoffee == null)
        {
            ShowValidationMessage(
                "Önce bir kahve seçin."
            );

            return;
        }


        if (selectedCoffee.CoffeeType ==
            CoffeeType.Espresso &&
            button.ExtraName ==
            "Ekstra Espresso")
        {
            ShowValidationMessage(
                "Espresso için Ekstra Espresso seçilemez. Tek Shot veya Double Shot kullanın."
            );

            return;
        }


        if (selectedExtras.Contains(button))
        {
            selectedExtras.Remove(
                button
            );
        }
        else
        {
            selectedExtras.Add(
                button
            );
        }


        UpdateBasketDisplay();
    }


    // =========================================================
    // ÖDEME YÖNTEMİ
    // =========================================================

    public void SelectPaymentMethod(
        string method)
    {
        selectedPaymentMethod =
            method;


        if (paymentMethodText != null)
        {
            paymentMethodText.text =
                method;
        }


        ClearValidationMessage();
    }


    // =========================================================
    // SEPETİ GÜNCELLE
    // =========================================================

    private void UpdateBasketDisplay()
    {
        float total = 0f;

        string lines = "";


        if (selectedCoffee != null)
        {
            if (selectedCoffee.CoffeeType ==
                CoffeeType.Espresso)
            {
                if (selectedEspressoShot != null)
                {
                    float price;


                    if (selectedEspressoShot.Shot ==
                        EspressoShotButtonUI.ShotType.Single)
                    {
                        price = 2.15f;


                        lines +=
                            $"Espresso - Tek Shot " +
                            $"{price:0.00}$\n";
                    }
                    else
                    {
                        price = 2.75f;


                        lines +=
                            $"Espresso - Double Shot " +
                            $"{price:0.00}$\n";
                    }


                    total += price;
                }
            }
            else if (selectedSize.HasValue)
            {
                float price =
                    selectedCoffee.GetPrice(
                        selectedSize.Value
                    );


                lines +=
                    $"{SizeToTurkish(selectedSize.Value)} " +
                    $"{selectedCoffee.CoffeeName} " +
                    $"{price:0.00}$\n";


                total += price;
            }
        }


        foreach (
            ExtraButtonUI extra
            in selectedExtras)
        {
            lines +=
                $"{extra.ExtraName} " +
                $"{extra.Price:0.00}$\n";


            total +=
                extra.Price;
        }


        if (basketText != null)
        {
            basketText.text =
                lines;
        }


        if (totalText != null)
        {
            totalText.text =
                $"Toplam: {total:0.00}$";
        }
    }


    // =========================================================
    // SEPETİ SIFIRLA
    // =========================================================

    public void ResetBasket()
    {
        selectedCoffee = null;

        selectedSize = null;

        selectedEspressoShot = null;

        selectedExtras.Clear();

        selectedPaymentMethod = "";


        if (paymentMethodText != null)
        {
            paymentMethodText.text = "";
        }


        UpdateBasketDisplay();


        ClearValidationMessage();
    }


    // =========================================================
    // SİPARİŞİ ONAYLA
    // =========================================================

    public void ConfirmOrder()
    {
        if (currentTargetOrder == null)
        {
            ShowValidationMessage(
                "Şu anda bekleyen bir müşteri yok."
            );

            return;
        }


        if (selectedCoffee == null)
        {
            ShowValidationMessage(
                "Lütfen bir kahve seçin."
            );

            return;
        }


        if (!selectedCoffee.IsUnlocked)
        {
            ShowValidationMessage(
                "Bu kahve henüz açılmadı."
            );

            return;
        }


        if (selectedCoffee.CoffeeType ==
            CoffeeType.Espresso)
        {
            if (selectedEspressoShot == null)
            {
                ShowValidationMessage(
                    "Lütfen Tek Shot veya Double Shot seçin."
                );

                return;
            }


            selectedSize = null;


            foreach (
                ExtraButtonUI extra
                in selectedExtras)
            {
                if (extra != null &&
                    extra.ExtraName ==
                    "Ekstra Espresso")
                {
                    ShowValidationMessage(
                        "Espresso için Ekstra Espresso kullanılamaz."
                    );

                    return;
                }
            }
        }
        else
        {
            if (!selectedSize.HasValue)
            {
                ShowValidationMessage(
                    "Lütfen kahve boyutu seçin."
                );

                return;
            }
        }


        foreach (
            ExtraButtonUI extra
            in selectedExtras)
        {
            if (extra == null)
                continue;


            if (extra.IsLocked)
            {
                ShowValidationMessage(
                    "Seçtiğiniz ekstralardan biri henüz açılmadı."
                );

                return;
            }
        }


        if (string.IsNullOrEmpty(
            selectedPaymentMethod))
        {
            ShowValidationMessage(
                "Lütfen ödeme yöntemi seçin."
            );

            return;
        }


        float total = 0f;


        if (selectedCoffee.CoffeeType ==
            CoffeeType.Espresso)
        {
            if (selectedEspressoShot.Shot ==
                EspressoShotButtonUI.ShotType.Single)
            {
                total = 2.15f;
            }
            else
            {
                total = 2.75f;
            }
        }
        else
        {
            total =
                selectedCoffee.GetPrice(
                    selectedSize.Value
                );
        }


        foreach (
            ExtraButtonUI extra
            in selectedExtras)
        {
            total +=
                extra.Price;
        }


        // =====================================================
        // OYUNCUNUN HAZIRLADIĞI SİPARİŞ
        // =====================================================

        Order finalOrder =
            new Order
            {
                coffeeType =
                    selectedCoffee.CoffeeType,

                size =
                    selectedCoffee.CoffeeType ==
                    CoffeeType.Espresso
                        ? CupSize.Small
                        : selectedSize.Value,

                espressoShot =
                    selectedEspressoShot != null
                        ? selectedEspressoShot.Shot
                        : EspressoShotButtonUI.ShotType.Single,

                reward =
                    Mathf.RoundToInt(total),

                // Yanlış teslimatta bu tutar kadar para geri kesilecek.
                pricePaid =
                    total,

                timeLimit =
                    orderTimeLimit,

                preferredPaymentMethod =
                    selectedPaymentMethod
            };


        foreach (
            ExtraButtonUI extra
            in selectedExtras)
        {
            finalOrder.requestedExtras.Add(
                extra.ExtraName
            );
        }


        if (OrderUI.Instance != null)
        {
            OrderUI.Instance.AddOrder(
                finalOrder
            );
        }


        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(
                total
            );


            Debug.Log(
                $"Sipariş parası eklendi: " +
                $"{total:0.00}$"
            );
        }
        else
        {
            Debug.LogWarning(
                "MoneyManager.Instance bulunamadı!"
            );
        }


        if (currentCustomerNPC != null)
        {
            currentCustomerNPC.ConfirmCustomerOrder();
        }
        else
        {
            Debug.LogWarning(
                "Confirm edilecek NPC bulunamadı! " +
                "(currentCustomerNPC null — " +
                "SetCustomerOrder çağrılırken NPC referansı " +
                "gönderilmemiş olabilir.)"
            );
        }


        string summary =
            selectedCoffee.CoffeeName;


        if (selectedCoffee.CoffeeType ==
            CoffeeType.Espresso)
        {
            if (selectedEspressoShot.Shot ==
                EspressoShotButtonUI.ShotType.Single)
            {
                summary +=
                    " + Tek Shot";
            }
            else
            {
                summary +=
                    " + Double Shot";
            }
        }
        else
        {
            summary =
                $"{SizeToTurkish(selectedSize.Value)} " +
                $"{summary}";
        }


        foreach (
            ExtraButtonUI extra
            in selectedExtras)
        {
            summary +=
                $" + {extra.ExtraName}";
        }


        Debug.Log(
            $"Sipariş onaylandı: " +
            $"{summary} - " +
            $"{selectedPaymentMethod} - " +
            $"{total:0.00}$"
        );


        currentTargetOrder = null;

        currentCustomerNPC = null;


        ResetBasket();
    }


    // =========================================================
    // UYARI GÖSTER
    // =========================================================

    private Coroutine validationCoroutine;


    private void ShowValidationMessage(
        string message)
    {
        if (validationText == null)
            return;


        if (validationCoroutine != null)
        {
            StopCoroutine(
                validationCoroutine
            );
        }


        validationText.gameObject.SetActive(
            true
        );


        validationText.text =
            message;


        if (validationBackground != null)
        {
            validationBackground.SetActive(
                true
            );
        }


        validationCoroutine =
            StartCoroutine(
                HideValidationMessage()
            );
    }


    private IEnumerator HideValidationMessage()
    {
        yield return new WaitForSeconds(
            3f
        );


        if (validationText != null)
        {
            validationText.text =
                "";

            validationText.gameObject.SetActive(
                false
            );
        }


        if (validationBackground != null)
        {
            validationBackground.SetActive(
                false
            );
        }


        validationCoroutine = null;
    }


    private void ClearValidationMessage()
    {
        if (validationCoroutine != null)
        {
            StopCoroutine(
                validationCoroutine
            );

            validationCoroutine = null;
        }


        if (validationBackground != null)
        {
            validationBackground.SetActive(
                false
            );
        }


        if (validationText != null)
        {
            validationText.text =
                "";

            validationText.gameObject.SetActive(
                false
            );
        }
    }


    // =========================================================
    // KAHVE TÜRÜNÜ TÜRKÇEYE ÇEVİR
    // =========================================================

    private string TypeToTurkish(
        CoffeeType type)
    {
        switch (type)
        {
            case CoffeeType.Espresso:
                return "Espresso";

            case CoffeeType.Latte:
                return "Latte";

            case CoffeeType.Cappuccino:
                return "Cappuccino";

            case CoffeeType.Americano:
                return "Americano";
        }


        return "";
    }


    // =========================================================
    // BOYUTU TÜRKÇEYE ÇEVİR
    // =========================================================

    private string SizeToTurkish(
        CupSize size)
    {
        switch (size)
        {
            case CupSize.Small:
                return "Küçük";

            case CupSize.Medium:
                return "Orta";

            case CupSize.Large:
                return "Büyük";
        }


        return "";
    }
}