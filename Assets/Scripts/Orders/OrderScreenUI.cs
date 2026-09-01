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


    // NPC'nin gerçek siparişi
    private Order currentTargetOrder;


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
        Order order)
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


        ResetBasket();


        UpdateCustomerRequestText();


        Debug.Log(
            "Kasa ekranına NPC siparişi geldi: " +
            order.coffeeType
        );
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


        // =====================================================
        // ESPRESSO
        // =====================================================

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


        // =====================================================
        // NORMAL KAHVELER
        // =====================================================

        else
        {
            coffeePart =
                $"{SizeToTurkish(currentTargetOrder.size)} " +
                $"{TypeToTurkish(currentTargetOrder.coffeeType)}";
        }


        // =====================================================
        // EKSTRA
        // =====================================================

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


        // =====================================================
        // YAZI
        // =====================================================

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


        // Espresso'da boyut seçilmez.
        if (IsEspressoSelected())
        {
            ShowValidationMessage(
                "Espresso her zaman küçük bardakta hazırlanır."
            );

            return;
        }


        // Shot seçilmişse boyut seçilemez.
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


        // =====================================================
        // KİLİT KONTROLÜ
        // =====================================================

        if (!button.IsUnlocked)
        {
            ShowValidationMessage(
                "Bu kahve henüz açılmadı."
            );

            return;
        }


        // =====================================================
        // SHOT SEÇİMİYLE ÇAKIŞMA
        // =====================================================

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


        // =====================================================
        // ESPRESSO
        // =====================================================

        if (button.CoffeeType ==
            CoffeeType.Espresso)
        {
            // Espresso her zaman küçük.
            selectedSize = null;
        }


        // =====================================================
        // NORMAL KAHVE
        // =====================================================

        else
        {
            // Normal kahve seçilince
            // Espresso shot temizlenir.
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


        // Önce kahve seçilmeli.
        if (selectedCoffee == null)
        {
            ShowValidationMessage(
                "Önce Espresso seçin."
            );

            return;
        }


        // Sadece Espresso'da.
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


        // Espresso'da boyut yok.
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


        // Kilitli ekstra
        if (button.IsLocked)
            return;


        // =====================================================
        // ÖNCE KAHVE SEÇİLMELİ
        // =====================================================

        if (selectedCoffee == null)
        {
            ShowValidationMessage(
                "Önce bir kahve seçin."
            );

            return;
        }


        // =====================================================
        // ESPRESSO + EKSTRA ESPRESSO YASAK
        // =====================================================

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


        // =====================================================
        // EKSTRA EKLE / ÇIKAR
        // =====================================================

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


        // =====================================================
        // KAHVE
        // =====================================================

        if (selectedCoffee != null)
        {
            // -------------------------------------------------
            // ESPRESSO
            // -------------------------------------------------

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


            // -------------------------------------------------
            // NORMAL KAHVELER
            // -------------------------------------------------

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


        // =====================================================
        // EKSTRALAR
        // =====================================================

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


        // =====================================================
        // UI
        // =====================================================

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
        // =====================================================
        // MÜŞTERİ VAR MI?
        // =====================================================

        if (currentTargetOrder == null)
        {
            ShowValidationMessage(
                "Şu anda bekleyen bir müşteri yok."
            );

            return;
        }


        // =====================================================
        // KAHVE SEÇİLDİ Mİ?
        // =====================================================

        if (selectedCoffee == null)
        {
            ShowValidationMessage(
                "Lütfen bir kahve seçin."
            );

            return;
        }


        // =====================================================
        // KAHVE HÂLÂ AÇIK MI?
        // =====================================================

        if (!selectedCoffee.IsUnlocked)
        {
            ShowValidationMessage(
                "Bu kahve henüz açılmadı."
            );

            return;
        }


        // =====================================================
        // ESPRESSO / NORMAL KAHVE
        // =====================================================

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


            // Espresso daima küçük.
            selectedSize = null;


            // Espresso + Ekstra Espresso yasak.
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


        // =====================================================
        // EKSTRALARIN KİLİT KONTROLÜ
        // =====================================================

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


        // =====================================================
        // ÖDEME YÖNTEMİ
        // =====================================================

        if (string.IsNullOrEmpty(
            selectedPaymentMethod))
        {
            ShowValidationMessage(
                "Lütfen ödeme yöntemi seçin."
            );

            return;
        }


        // =====================================================
        // FİYAT
        // =====================================================

        float total = 0f;


        // =====================================================
        // ESPRESSO
        // =====================================================

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


        // =====================================================
        // NORMAL KAHVE
        // =====================================================

        else
        {
            total =
                selectedCoffee.GetPrice(
                    selectedSize.Value
                );
        }


        // =====================================================
        // EKSTRA FİYATLARI
        // =====================================================

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

                timeLimit =
                    orderTimeLimit,

                preferredPaymentMethod =
                    selectedPaymentMethod
            };


        // =====================================================
        // EKSTRALARI EKLE
        // =====================================================

        foreach (
            ExtraButtonUI extra
            in selectedExtras)
        {
            finalOrder.requestedExtras.Add(
                extra.ExtraName
            );
        }


        // =====================================================
        // ORDER UI
        // =====================================================

        if (OrderUI.Instance != null)
        {
            OrderUI.Instance.AddOrder(
                finalOrder
            );
        }


        // =====================================================
        // PARA
        // =====================================================

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


        // =====================================================
        // NPC
        // =====================================================

        NPCController currentCustomer =
            FindObjectOfType<NPCController>();


        if (currentCustomer != null)
        {
            currentCustomer.ConfirmCustomerOrder();
        }
        else
        {
            Debug.LogWarning(
                "NPCController bulunamadı!"
            );
        }


        // =====================================================
        // SİPARİŞ ÖZETİ
        // =====================================================

        string summary =
            selectedCoffee.CoffeeName;


        // =====================================================
        // ESPRESSO ÖZETİ
        // =====================================================

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


        // =====================================================
        // NORMAL KAHVE ÖZETİ
        // =====================================================

        else
        {
            summary =
                $"{SizeToTurkish(selectedSize.Value)} " +
                $"{summary}";
        }


        // =====================================================
        // EKSTRA ÖZETİ
        // =====================================================

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


        // =====================================================
        // SEPETİ TEMİZLE
        // =====================================================

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