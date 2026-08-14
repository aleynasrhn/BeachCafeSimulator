using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Kasa ekranındaki tüm sipariş seçimlerini yönetir.
///
/// Normal kahveler:
/// Kahve + Boyut + Ödeme + İsteğe bağlı ekstralar
///
/// Espresso:
/// Espresso + Tek Shot / Double Shot + Ödeme + İsteğe bağlı ekstralar
/// </summary>
public class OrderScreenUI : MonoBehaviour
{
    [Header("Müşteri Talebi")]
    [SerializeField] private TMP_Text customerRequestText;

    [Header("Doğrulama Mesajı")]
    [SerializeField] private TMP_Text validationText;
    [SerializeField] private GameObject validationBackground;

    [Header("Boyut Butonları (3 tane)")]
    [SerializeField] private SizeButtonUI[] sizeButtons;

    [Header("Espresso Shot Butonları (2 tane)")]
    [SerializeField] private EspressoShotButtonUI[] espressoShotButtons;

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


    // =========================================================
    // HAVUZLAR
    // =========================================================

    private static readonly string[] extraNamePool =
    {
        "Ekstra Espresso",
        "Tarçın",
        "Çikolata Şurubu",
        "Karamel Şurup",
        "Vanilya Şurubu"
    };

    private static readonly string[] paymentPool =
    {
        "Nakit Ödeme",
        "Kart Ödeme"
    };


    // =========================================================
    // EKRAN AÇILDIĞINDA
    // =========================================================

    private void OnEnable()
    {
        ResetBasket();
        GenerateNewCustomerRequest();
    }


    // =========================================================
    // MÜŞTERİ TALEBİ OLUŞTUR
    // =========================================================

    private void GenerateNewCustomerRequest()
    {
        CoffeeType randomCoffee =
            (CoffeeType)Random.Range(
                0,
                System.Enum.GetValues(typeof(CoffeeType)).Length
            );

        currentTargetOrder = new Order
        {
            coffeeType = randomCoffee,

            size = (CupSize)Random.Range(
                0,
                System.Enum.GetValues(typeof(CupSize)).Length
            ),

            reward = 0,

            timeLimit = orderTimeLimit,

            preferredPaymentMethod =
                paymentPool[
                    Random.Range(
                        0,
                        paymentPool.Length
                    )
                ]
        };


        // =====================================================
        // ESPRESSO İSE BOYUT YERİNE SHOT
        // =====================================================

        if (randomCoffee == CoffeeType.Espresso)
        {
            currentTargetOrder.espressoShot =
                Random.value > 0.5f
                    ? EspressoShotButtonUI.ShotType.Single
                    : EspressoShotButtonUI.ShotType.Double;
        }


        // =====================================================
        // %50 İHTİMALLE EKSTRA İSTE
        // =====================================================

        if (Random.value > 0.5f)
        {
            currentTargetOrder.requestedExtras.Add(
                extraNamePool[
                    Random.Range(
                        0,
                        extraNamePool.Length
                    )
                ]
            );
        }


        UpdateCustomerRequestText();
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
                shotName = "Tek Shot";
            }
            else
            {
                shotName = "Double Shot";
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

        if (currentTargetOrder.requestedExtras.Count > 0)
        {
            extrasPart =
                ", " +
                string.Join(
                    ", ",
                    currentTargetOrder.requestedExtras
                );
        }


        // =====================================================
        // ÖDEME + TALEP
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

    public void SelectSize(SizeButtonUI button)
    {
        if (button == null)
            return;


        // Espresso'da boyut yok
        if (IsEspressoSelected())
        {
            ShowValidationMessage(
                "Espresso için Tek Shot veya Double Shot seçin."
            );

            return;
        }


        // Shot seçilmişse boyut seçilemez
        if (selectedEspressoShot != null)
        {
            ShowValidationMessage(
                "Espresso seçmeden shot seçemezsiniz."
            );

            return;
        }


        selectedSize = button.Size;

        ClearValidationMessage();

        UpdateBasketDisplay();
    }


    // =========================================================
    // KAHVE SEÇ
    // =========================================================

    public void SelectCoffee(CoffeeButtonUI button)
    {
        if (button == null)
            return;


        // Shot seçildiyse sadece Espresso seçilebilir
        if (selectedEspressoShot != null &&
            button.CoffeeType != CoffeeType.Espresso)
        {
            ShowValidationMessage(
                "Tek Shot ve Double Shot sadece Espresso için kullanılabilir."
            );

            return;
        }


        selectedCoffee = button;


        // =====================================================
        // ESPRESSO
        // =====================================================

        if (button.CoffeeType ==
            CoffeeType.Espresso)
        {
            // Espresso'da boyut kullanılmaz
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


        // Önce kahve seçilmeli
        if (selectedCoffee == null)
        {
            ShowValidationMessage(
                "Önce Espresso seçin."
            );

            return;
        }


        // Sadece Espresso'da kullanılabilir
        if (selectedCoffee.CoffeeType !=
            CoffeeType.Espresso)
        {
            ShowValidationMessage(
                "Tek Shot ve Double Shot sadece Espresso için kullanılabilir."
            );

            return;
        }


        selectedEspressoShot = button;

        // Espresso'da boyut yok
        selectedSize = null;

        ClearValidationMessage();

        UpdateBasketDisplay();
    }


    // =========================================================
    // EKSTRA SEÇ / ÇIKAR
    // =========================================================

    public void ToggleExtra(ExtraButtonUI button)
    {
        if (button == null)
            return;

        if (button.IsLocked)
            return;


        if (selectedExtras.Contains(button))
        {
            selectedExtras.Remove(button);
        }
        else
        {
            selectedExtras.Add(button);
        }


        UpdateBasketDisplay();
    }


    // =========================================================
    // ÖDEME YÖNTEMİ
    // =========================================================

    public void SelectPaymentMethod(string method)
    {
        selectedPaymentMethod = method;

        if (paymentMethodText != null)
        {
            paymentMethodText.text = method;
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

        foreach (var extra in selectedExtras)
        {
            lines +=
                $"{extra.ExtraName} " +
                $"{extra.Price:0.00}$\n";

            total += extra.Price;
        }


        // =====================================================
        // UI
        // =====================================================

        if (basketText != null)
        {
            basketText.text = lines;
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
        // 1 - KAHVE SEÇİLDİ Mİ?
        // =====================================================

        if (selectedCoffee == null)
        {
            ShowValidationMessage(
                "Lütfen bir kahve seçin."
            );

            return;
        }


        // =====================================================
        // 2 - ESPRESSO / NORMAL KAHVE KONTROLÜ
        // =====================================================

        // -----------------------------------------------------
        // ESPRESSO
        // -----------------------------------------------------

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
        }


        // -----------------------------------------------------
        // NORMAL KAHVE
        // -----------------------------------------------------

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
        // 3 - ÖDEME YÖNTEMİ KONTROLÜ
        // =====================================================

        if (string.IsNullOrEmpty(selectedPaymentMethod))
        {
            ShowValidationMessage(
                "Lütfen ödeme yöntemi seçin."
            );

            return;
        }


        // =====================================================
        // 4 - FİYAT HESAPLA
        // =====================================================

        float total = 0f;


        // -----------------------------------------------------
        // ESPRESSO FİYATI
        // -----------------------------------------------------

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


        // -----------------------------------------------------
        // NORMAL KAHVE FİYATI
        // -----------------------------------------------------

        else
        {
            total =
                selectedCoffee.GetPrice(
                    selectedSize.Value
                );
        }


        // =====================================================
        // 5 - EKSTRA FİYATLARI
        // =====================================================

        // Ekstra seçmek zorunlu değil.
        // Seçilmiş olanların fiyatı eklenir.

        foreach (var extra in selectedExtras)
        {
            total += extra.Price;
        }


        // =====================================================
        // 6 - ORDER OLUŞTUR
        // =====================================================

        Order finalOrder = new Order
        {
            coffeeType =
                selectedCoffee.CoffeeType,

            // Normal kahvelerde gerçek boyut.
            // Espresso'da mevcut Order sistemi size istediği
            // için şimdilik Small gönderiyoruz.
            size =
                selectedSize ?? CupSize.Small,

            // Espresso Shot
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
        // 7 - SAĞ SİPARİŞ PANELİNE EKLE
        // =====================================================

        if (OrderUI.Instance != null)
        {
            OrderUI.Instance.AddOrder(finalOrder);
        }


        // =====================================================
        // 8 - PARA EKLE
        // =====================================================

        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(total);
        }


        // =====================================================
        // 9 - SİPARİŞ ÖZETİ
        // =====================================================

        string summary =
            selectedCoffee.CoffeeName;


        // -----------------------------------------------------
        // ESPRESSO ÖZETİ
        // -----------------------------------------------------

        if (selectedCoffee.CoffeeType ==
            CoffeeType.Espresso)
        {
            if (selectedEspressoShot.Shot ==
                EspressoShotButtonUI.ShotType.Single)
            {
                summary += " + Tek Shot";
            }
            else
            {
                summary += " + Double Shot";
            }
        }


        // -----------------------------------------------------
        // NORMAL KAHVE ÖZETİ
        // -----------------------------------------------------

        else
        {
            summary =
                $"{SizeToTurkish(selectedSize.Value)} " +
                $"{summary}";
        }


        // -----------------------------------------------------
        // EKSTRA ÖZETİ
        // -----------------------------------------------------

        foreach (var extra in selectedExtras)
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
        // 10 - SIFIRLA
        // =====================================================

        ResetBasket();


        // =====================================================
        // 11 - YENİ MÜŞTERİ TALEBİ
        // =====================================================

        GenerateNewCustomerRequest();
    }


    // =========================================================
    // UYARI GÖSTER
    // =========================================================

    private Coroutine validationCoroutine;

    private void ShowValidationMessage(string message)
    {
        if (validationText == null)
            return;


        if (validationCoroutine != null)
        {
            StopCoroutine(validationCoroutine);
        }


        validationText.gameObject.SetActive(true);

        validationText.text = message;


        if (validationBackground != null)
        {
            validationBackground.SetActive(true);
        }


        validationCoroutine =
            StartCoroutine(HideValidationMessage());
    }


    private IEnumerator HideValidationMessage()
    {
        yield return new WaitForSeconds(3f);


        if (validationText != null)
        {
            validationText.text = "";

            validationText.gameObject.SetActive(false);
        }


        if (validationBackground != null)
        {
            validationBackground.SetActive(false);
        }


        validationCoroutine = null;
    }


    private void ClearValidationMessage()
    {
        if (validationCoroutine != null)
        {
            StopCoroutine(validationCoroutine);

            validationCoroutine = null;
        }


        if (validationBackground != null)
        {
            validationBackground.SetActive(false);
        }


        if (validationText != null)
        {
            validationText.text = "";

            validationText.gameObject.SetActive(false);
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