using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Fiziksel POS (ödeme) cihazını yönetir.
/// Sadece KART ödemesi seçildiğinde devreye girer.
///
/// Giriş yöntemleri:
/// - Klavye
/// - Mouse ile fiziksel POS tuşlarına tıklama
///
/// Akış:
/// 1) OrderScreenUI, kart ödemesi onaylanınca
///    BeginTransaction(total, onSuccess) çağırır.
///
/// 2) Eğer oyuncu kasadaysa ComputerInteraction kamerayı
///    POS sistemine devreder.
///
/// 3) Kamera POS cihazına gider.
///
/// 4) Oyuncu:
///    - Klavyeden rakam girebilir.
///    - Mouse ile fiziksel POS tuşlarına basabilir.
///
/// 5) Yeşil buton / Enter:
///    "İşlem yapılıyor..."
///
/// 6) Tutar doğruysa:
///    - İşlem Onaylandı
///    - Para eklenir
///    - NPC onaylanır
///    - Kamera geri döner
///
/// 7) Tutar yanlışsa:
///    - İşlem Onaylanmadı
///    - Tutar temizlenir
///    - POS ekranında kalınır
///    - Tekrar denenebilir
/// </summary>
public class PosMachineController : MonoBehaviour
{
    public static PosMachineController Instance;

    [Header("Kamera")]
    [SerializeField]
    private Transform cameraHolder;

    [SerializeField]
    private Transform posCameraPoint;


    [Header("Oyuncu")]
    [SerializeField]
    private PlayerMovement playerMovement;


    [Header("Kamera Ayarları")]
    [SerializeField]
    private float cameraMoveSpeed = 6f;


    [Header("Mouse POS Raycast")]
    [SerializeField]
    private float mouseRayDistance = 10f;


    [Header("Ekran Yazıları")]
    [SerializeField]
    private TMP_Text totalText;

    [SerializeField]
    private TMP_Text enteredAmountText;

    [SerializeField]
    private TMP_Text statusText;


    [Header("Zamanlama")]
    [SerializeField]
    private float processingDuration = 2f;

    [SerializeField]
    private float wrongAmountMessageDuration = 2f;

    [SerializeField]
    private float successMessageDuration = 1f;


    [Header("Tutar Karşılaştırma Toleransı")]
    [SerializeField]
    private float amountTolerance = 0.001f;


    // =========================================================
    // DURUM
    // =========================================================

    private bool isUsingPos = false;

    private bool isReturning = false;

    private Vector3 originalCameraPosition;

    private Quaternion originalCameraRotation;

    private float expectedTotal;

    private string enteredDigits = "";

    private bool isProcessing = false;

    private Action pendingOnSuccess;


    public bool IsUsingPos => isUsingPos;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        Instance = this;
    }


    // =========================================================
    // İŞLEMİ BAŞLAT
    // =========================================================

    public void BeginTransaction(
        float total,
        Action onSuccess)
    {
        if (isUsingPos || isReturning)
        {
            Debug.LogWarning(
                "PosMachineController: " +
                "Zaten bir işlem sürüyor, " +
                "yeni işlem başlatılamadı."
            );

            return;
        }


        if (cameraHolder == null ||
            posCameraPoint == null)
        {
            Debug.LogError(
                "PosMachineController: " +
                "cameraHolder veya posCameraPoint atanmamış! " +
                "İşlem başlatılamıyor."
            );

            return;
        }


        expectedTotal = total;

        pendingOnSuccess = onSuccess;

        enteredDigits = "";

        isProcessing = false;


        UpdateTotalDisplay();

        UpdateEnteredAmountDisplay();

        ClearStatusText();


        // =====================================================
        // KAMERA KONTROLÜNÜ DEVRAL
        // =====================================================

        if (ComputerInteraction.Instance != null &&
            ComputerInteraction.Instance.IsUsingComputer)
        {
            ComputerInteraction.Instance.HandOffCameraControl(
                out originalCameraPosition,
                out originalCameraRotation
            );
        }
        else
        {
            originalCameraPosition =
                cameraHolder.position;

            originalCameraRotation =
                cameraHolder.rotation;
        }


        isUsingPos = true;


        // Oyuncu hareket edemesin
        if (playerMovement != null)
        {
            playerMovement.canMove = false;
        }


        // =====================================================
        // MOUSE'U AKTİF ET
        // =====================================================

        Cursor.visible = true;

        Cursor.lockState =
            CursorLockMode.None;


        Debug.Log(
            $"POS işlemi başladı: {expectedTotal:0.00}$"
        );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (isUsingPos)
        {
            MoveCameraToPos();

            HandleKeyboardInput();

            HandleMouseInput();
        }


        if (isReturning)
        {
            ReturnCameraToOriginalPosition();
        }
    }


    // =========================================================
    // KLAVYE GİRİŞİ
    // =========================================================

    private void HandleKeyboardInput()
    {
        if (isProcessing)
        {
            return;
        }


        // -----------------------------------------------------
        // RAKAMLAR
        // -----------------------------------------------------

        for (
            KeyCode key = KeyCode.Alpha0;
            key <= KeyCode.Alpha9;
            key++)
        {
            if (Input.GetKeyDown(key))
            {
                string digit =
                    ((int)(key - KeyCode.Alpha0))
                    .ToString();

                OnDigitPressed(digit);
            }
        }


        // -----------------------------------------------------
        // NUMPAD RAKAMLARI
        // -----------------------------------------------------

        for (
            KeyCode key = KeyCode.Keypad0;
            key <= KeyCode.Keypad9;
            key++)
        {
            if (Input.GetKeyDown(key))
            {
                string digit =
                    ((int)(key - KeyCode.Keypad0))
                    .ToString();

                OnDigitPressed(digit);
            }
        }


        // -----------------------------------------------------
        // ONDALIK
        // -----------------------------------------------------

        if (Input.GetKeyDown(KeyCode.Period) ||
            Input.GetKeyDown(KeyCode.Comma) ||
            Input.GetKeyDown(KeyCode.KeypadPeriod))
        {
            OnDecimalPressed();
        }


        // -----------------------------------------------------
        // BACKSPACE
        // -----------------------------------------------------

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            OnBackspacePressed();
        }


        // -----------------------------------------------------
        // DELETE = TEMİZLE
        // -----------------------------------------------------

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            OnClearPressed();
        }


        // -----------------------------------------------------
        // ENTER
        // -----------------------------------------------------

        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnConfirmPressed();
        }


        // -----------------------------------------------------
        // ESC
        // -----------------------------------------------------

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelTransaction();
        }
    }


    // =========================================================
    // MOUSE GİRİŞİ
    // =========================================================

    private void HandleMouseInput()
    {
        if (isProcessing)
        {
            return;
        }


        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }


        if (cameraHolder == null)
        {
            return;
        }


        Camera currentCamera =
            cameraHolder.GetComponent<Camera>();


        if (currentCamera == null)
        {
            currentCamera =
                cameraHolder.GetComponentInChildren<Camera>();
        }


        if (currentCamera == null)
        {
            Debug.LogWarning(
                "PosMachineController: " +
                "cameraHolder üzerinde Camera bulunamadı!"
            );

            return;
        }


        Ray ray =
            currentCamera.ScreenPointToRay(
                Input.mousePosition
            );


        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            mouseRayDistance))
        {
            return;
        }


        PosButtonInteractable button =
            hit.collider.GetComponentInParent<
                PosButtonInteractable
            >();


        if (button != null)
        {
            button.MouseInteract();
        }
    }


    // =========================================================
    // RAKAM GİR
    // =========================================================

    public void OnDigitPressed(string digit)
    {
        if (!isUsingPos ||
            isProcessing)
        {
            return;
        }


        if (string.IsNullOrEmpty(digit))
        {
            return;
        }


        if (digit.Length != 1 ||
            !char.IsDigit(digit[0]))
        {
            return;
        }


        // Ondalık bölümde maksimum 2 hane
        int dotIndex =
            enteredDigits.IndexOf('.');


        if (dotIndex >= 0)
        {
            int decimals =
                enteredDigits.Length -
                dotIndex -
                1;


            if (decimals >= 2)
            {
                return;
            }
        }


        // Çok uzun giriş engeli
        if (enteredDigits.Length >= 9)
        {
            return;
        }


        enteredDigits += digit;


        UpdateEnteredAmountDisplay();
    }


    // =========================================================
    // ONDALIK AYIRICI
    // =========================================================

    public void OnDecimalPressed()
    {
        if (!isUsingPos ||
            isProcessing)
        {
            return;
        }


        if (enteredDigits.Contains("."))
        {
            return;
        }


        if (enteredDigits.Length == 0)
        {
            enteredDigits = "0.";
        }
        else
        {
            enteredDigits += ".";
        }


        UpdateEnteredAmountDisplay();
    }


    // =========================================================
    // GERİ SİL
    // =========================================================

    public void OnBackspacePressed()
    {
        if (!isUsingPos ||
            isProcessing)
        {
            return;
        }


        if (enteredDigits.Length > 0)
        {
            enteredDigits =
                enteredDigits.Substring(
                    0,
                    enteredDigits.Length - 1
                );
        }


        UpdateEnteredAmountDisplay();
    }


    // =========================================================
    // TEMİZLE
    // =========================================================

    public void OnClearPressed()
    {
        if (!isUsingPos ||
            isProcessing)
        {
            return;
        }


        enteredDigits = "";


        UpdateEnteredAmountDisplay();
    }


    // =========================================================
    // ONAYLA
    // =========================================================

    public void OnConfirmPressed()
    {
        if (!isUsingPos ||
            isProcessing)
        {
            return;
        }


        StartCoroutine(
            ProcessTransaction()
        );
    }


    // =========================================================
    // İŞLEM SÜRECİ
    // =========================================================

    private IEnumerator ProcessTransaction()
    {
        isProcessing = true;


        ShowStatusText(
            "İşlem yapılıyor..."
        );


        yield return new WaitForSeconds(
            processingDuration
        );


        float enteredAmount =
            ParseEnteredAmount();


        bool isCorrect =
            Mathf.Abs(
                enteredAmount -
                expectedTotal
            ) <= amountTolerance;


        // =====================================================
        // DOĞRU TUTAR
        // =====================================================

        if (isCorrect)
        {
            ShowStatusText(
                "İşlem Onaylandı"
            );


            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.AddMoney(
                    expectedTotal
                );


                Debug.Log(
                    $"POS ile ödeme alındı: " +
                    $"{expectedTotal:0.00}$"
                );
            }
            else
            {
                Debug.LogWarning(
                    "MoneyManager.Instance bulunamadı! " +
                    "POS ödemesi eklenemedi."
                );
            }


            yield return new WaitForSeconds(
                successMessageDuration
            );


            Action callback =
                pendingOnSuccess;


            pendingOnSuccess = null;


            callback?.Invoke();


            EndTransactionAndReturnCamera();


            // IEnumerator olduğu için return değil,
            // yield break kullanılmalı.
            yield break;
        }


        // =====================================================
        // YANLIŞ TUTAR
        // =====================================================

        ShowStatusText(
            "İşlem Onaylanmadı : " +
            "Lütfen Doğru Tutar Giriniz"
        );


        yield return new WaitForSeconds(
            wrongAmountMessageDuration
        );


        enteredDigits = "";


        UpdateEnteredAmountDisplay();


        ClearStatusText();


        isProcessing = false;
    }


    // =========================================================
    // ESC İLE İPTAL
    // =========================================================

    private void CancelTransaction()
    {
        if (isProcessing)
        {
            return;
        }


        Debug.Log(
            "PosMachineController: " +
            "İşlem iptal edildi, " +
            "müşteri hâlâ sırada bekliyor."
        );


        pendingOnSuccess = null;


        EndTransactionAndReturnCamera();
    }


    // =========================================================
    // İŞLEMİ BİTİR
    // =========================================================

    private void EndTransactionAndReturnCamera()
    {
        isUsingPos = false;

        isReturning = true;


        if (playerMovement != null)
        {
            playerMovement.canMove = false;
        }
    }


    // =========================================================
    // TUTARI PARSE ET
    // =========================================================

    private float ParseEnteredAmount()
    {
        if (string.IsNullOrEmpty(enteredDigits))
        {
            return 0f;
        }


        string normalized =
            enteredDigits;


        if (normalized.EndsWith("."))
        {
            normalized += "0";
        }


        if (float.TryParse(
            normalized,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out float result))
        {
            return result;
        }


        return 0f;
    }


    // =========================================================
    // EKRAN - TOTAL
    // =========================================================

    private void UpdateTotalDisplay()
    {
        if (totalText != null)
        {
            totalText.text =
                $"Total: {expectedTotal:0.00} $";
        }
    }


    // =========================================================
    // EKRAN - GİRİLEN TUTAR
    // =========================================================

    private void UpdateEnteredAmountDisplay()
    {
        if (enteredAmountText == null)
        {
            return;
        }


        string displayValue =
            string.IsNullOrEmpty(enteredDigits)
                ? "0.00"
                : enteredDigits;


        enteredAmountText.text =
            $"Lütfen ödenecek değeri giriniz: " +
            $"{displayValue} $";
    }


    // =========================================================
    // DURUM YAZISI
    // =========================================================

    private void ShowStatusText(
        string message)
    {
        if (statusText != null)
        {
            statusText.text =
                message;
        }
    }


    private void ClearStatusText()
    {
        if (statusText != null)
        {
            statusText.text = "";
        }
    }


    // =========================================================
    // KAMERA - POS'A GİT
    // =========================================================

    private void MoveCameraToPos()
    {
        if (cameraHolder == null ||
            posCameraPoint == null)
        {
            return;
        }


        cameraHolder.position =
            Vector3.Lerp(
                cameraHolder.position,
                posCameraPoint.position,
                Time.deltaTime *
                cameraMoveSpeed
            );


        cameraHolder.rotation =
            Quaternion.Lerp(
                cameraHolder.rotation,
                posCameraPoint.rotation,
                Time.deltaTime *
                cameraMoveSpeed
            );
    }


    // =========================================================
    // KAMERA - GERİ DÖN
    // =========================================================

    private void ReturnCameraToOriginalPosition()
    {
        if (cameraHolder == null)
        {
            return;
        }


        cameraHolder.position =
            Vector3.Lerp(
                cameraHolder.position,
                originalCameraPosition,
                Time.deltaTime *
                cameraMoveSpeed
            );


        cameraHolder.rotation =
            Quaternion.Lerp(
                cameraHolder.rotation,
                originalCameraRotation,
                Time.deltaTime *
                cameraMoveSpeed
            );


        float distance =
            Vector3.Distance(
                cameraHolder.position,
                originalCameraPosition
            );


        float angle =
            Quaternion.Angle(
                cameraHolder.rotation,
                originalCameraRotation
            );


        if (distance < 0.01f &&
            angle < 0.1f)
        {
            cameraHolder.position =
                originalCameraPosition;


            cameraHolder.rotation =
                originalCameraRotation;


            isReturning = false;


            if (playerMovement != null)
            {
                playerMovement.canMove = true;
            }


            // =================================================
            // MOUSE'U ESKİ HALE GETİR
            // =================================================

            Cursor.visible = false;

            Cursor.lockState =
                CursorLockMode.Locked;


            ClearStatusText();


            enteredDigits = "";


            UpdateEnteredAmountDisplay();


            isProcessing = false;
        }
    }
}