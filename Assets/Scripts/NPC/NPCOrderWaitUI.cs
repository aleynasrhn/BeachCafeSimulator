using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Müşterinin başında beliren, sipariş bekleme süresini
/// gösteren dolan bar + saniye yazısı + sipariş özeti.
///
/// Kamera ne yöne bakarsa baksın her zaman kameraya döner
/// (billboard) ve asla ters dönmez.
/// </summary>
public class NPCOrderWaitUI : MonoBehaviour
{
    [Header("UI Kökü (Canvas)")]
    [SerializeField] private GameObject uiRoot;

    [Header("Dolan Bar (Image - Fill Amount)")]
    [SerializeField] private Image fillImage;

    [Header("Saniye Yazısı")]
    [SerializeField] private TMP_Text timeText;

    [Header("Sipariş Özeti Yazısı (Barın Altında)")]
    [SerializeField] private TMP_Text orderText;

    [Header("Takip Ayarları")]
    [SerializeField] private Transform followTarget;

    [SerializeField]
    private Vector3 offset =
        new Vector3(0f, 2.2f, 0f);

    private Camera mainCamera;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        mainCamera =
            Camera.main;

        Hide();
    }


    // =========================================================
    // LATE UPDATE (KAMERAYA BAK + TAKİP ET)
    // =========================================================

    private void LateUpdate()
    {
        if (uiRoot == null ||
            !uiRoot.activeSelf)
        {
            return;
        }

        if (followTarget != null)
        {
            uiRoot.transform.position =
                followTarget.position +
                offset;
        }

        if (mainCamera == null)
        {
            mainCamera =
                Camera.main;
        }

        if (mainCamera != null)
        {
            // =================================================
            // TERS DÖNMEYEN BILLBOARD ROTASYONU
            // =================================================
            //
            // Sadece "forward" atamak, kameranın açısına göre
            // "up" vektörünün ters dönmesine (dolayısıyla
            // yazının baş aşağı görünmesine) sebep olabiliyordu.
            // LookRotation ile kameranın up'ını açıkça vererek
            // bu sorunu kalıcı olarak çözüyoruz.
            //
            // =================================================

            Vector3 directionAwayFromCamera =
                uiRoot.transform.position -
                mainCamera.transform.position;

            if (directionAwayFromCamera.sqrMagnitude > 0.0001f)
            {
                uiRoot.transform.rotation =
                    Quaternion.LookRotation(
                        directionAwayFromCamera,
                        mainCamera.transform.up
                    );
            }
        }
    }


    // =========================================================
    // GÖSTER / GİZLE
    // =========================================================

    public void Show()
    {
        if (uiRoot != null)
        {
            uiRoot.SetActive(true);
        }
    }

    public void Hide()
    {
        if (uiRoot != null)
        {
            uiRoot.SetActive(false);
        }
    }


    // =========================================================
    // İLERLEMEYİ GÜNCELLE
    // =========================================================
    //
    // progress01: 0 ile 1 arasında dolum oranı.
    // secondsRemaining: kalan saniye (yüzde değil, saniye
    // olarak gösterilir).
    //
    // =========================================================

    public void UpdateProgress(
        float progress01,
        float secondsRemaining)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount =
                Mathf.Clamp01(progress01);
        }

        if (timeText != null)
        {
            int seconds =
                Mathf.CeilToInt(
                    Mathf.Max(
                        0f,
                        secondsRemaining
                    )
                );

            timeText.text =
                $"{seconds} sn";
        }
    }


    // =========================================================
    // SİPARİŞ ÖZETİNİ AYARLA (BARIN ALTINDA GÖRÜNÜR)
    // =========================================================

    public void SetOrderText(string text)
    {
        if (orderText != null)
        {
            orderText.text = text;
        }
    }
}