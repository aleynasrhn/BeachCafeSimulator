using UnityEngine;
using System.Collections;

public class KettleHeatController : MonoBehaviour
{
    [Header("Kettle")]
    [SerializeField] private KettleWaterState waterState;
    [SerializeField] private PickupItem pickupItem;

    [Header("Kettle Dock")]
    [SerializeField] private KettleDockPoint kettleDockPoint;

    [Header("Kaynatma")]
    [SerializeField] private float heatDuration = 10f;

    [Header("Işıklar")]
    [SerializeField] private GameObject redLight;
    [SerializeField] private GameObject greenLight;

    private bool isHeating = false;
    private bool isHot = false;

    private Coroutine heatingCoroutine;


    public bool IsHeating => isHeating;
    public bool IsHot => isHot;


    // =========================================================
    // BAŞLANGIÇ
    // =========================================================

    private void Start()
    {
        if (waterState == null)
        {
            waterState =
                GetComponent<KettleWaterState>();
        }

        if (pickupItem == null)
        {
            pickupItem =
                GetComponent<PickupItem>();
        }

        isHeating = false;
        isHot = false;

        SetRedLight(false);
        SetGreenLight(false);
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (pickupItem == null)
            return;

        // Kettle eldeyse iki ışık da kapalı.
        if (pickupItem.IsHeld)
        {
            SetRedLight(false);
            SetGreenLight(false);

            return;
        }

        // BURADA YEŞİLİ TEKRAR AÇMIYORUZ.
        // Yeşil sadece kaynama tamamlandığı anda
        // HeatCoroutine tarafından açılacak.
    }


    // =========================================================
    // DOCK KONTROLÜ
    // =========================================================

    private bool IsKettleProperlyDocked()
    {
        if (kettleDockPoint == null)
            return false;

        if (pickupItem == null)
            return false;

        if (!kettleDockPoint.IsOccupied)
            return false;

        if (kettleDockPoint.DockedItem != pickupItem)
            return false;

        return true;
    }


    // =========================================================
    // KAYNATMA BAŞLAT
    // =========================================================

    public bool StartHeating()
    {
        if (waterState == null)
        {
            Debug.LogWarning(
                "KettleHeatController: KettleWaterState bulunamadı.",
                this
            );

            return false;
        }


        // Standda değilse çalışmaz
        if (!IsKettleProperlyDocked())
        {
            Debug.Log(
                "Kettle standına tam oturmadan çalıştırılamaz."
            );

            return false;
        }


        // Su yoksa çalışmaz
        if (!waterState.HasWater)
        {
            Debug.Log(
                "Kettle'da su yok."
            );

            return false;
        }


        // Zaten ısınıyorsa
        if (isHeating)
            return false;


        // Zaten sıcaksa
        if (isHot)
        {
            Debug.Log(
                "Kettle'daki su zaten sıcak."
            );

            return false;
        }


        if (heatingCoroutine != null)
        {
            StopCoroutine(
                heatingCoroutine
            );
        }


        heatingCoroutine =
            StartCoroutine(
                HeatCoroutine()
            );

        return true;
    }


    // =========================================================
    // KAYNATMA
    // =========================================================

    private IEnumerator HeatCoroutine()
    {
        isHeating = true;
        isHot = false;


        // Yeni kaynatma başlarken:
        SetRedLight(true);
        SetGreenLight(false);


        Debug.Log(
            "Kettle ısınıyor..."
        );


        float elapsed = 0f;


        while (elapsed < heatDuration)
        {
            elapsed += Time.deltaTime;

            yield return null;
        }


        // =====================================================
        // SICAK SU
        // =====================================================

        isHeating = false;
        isHot = true;


        waterState.SetHot();


        // Kırmızı söner
        SetRedLight(false);

        // Yeşil sadece TAM BURADA yanar.
        SetGreenLight(true);


        Debug.Log(
            "Kettle hazır! Su sıcak."
        );


        heatingCoroutine = null;
    }


    // =========================================================
    // KETTLE'I KAPAT
    // =========================================================

    public void StopHeating()
    {
        if (heatingCoroutine != null)
        {
            StopCoroutine(
                heatingCoroutine
            );

            heatingCoroutine = null;
        }


        isHeating = false;


        SetRedLight(false);


        // Eğer henüz sıcak değilse yeşili kapat.
        if (!isHot)
        {
            SetGreenLight(false);
        }


        Debug.Log(
            "Kettle kapatıldı."
        );
    }


    // =========================================================
    // IŞIKLAR
    // =========================================================

    private void SetRedLight(bool value)
    {
        if (redLight != null)
        {
            redLight.SetActive(value);
        }
    }


    private void SetGreenLight(bool value)
    {
        if (greenLight != null)
        {
            greenLight.SetActive(value);
        }
    }
}