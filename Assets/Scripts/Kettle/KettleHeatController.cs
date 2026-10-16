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

    [Header("Sayaç")]
    [SerializeField] private KettleTimerDisplay timerDisplay;

    [Header("Işıklar")]
    [SerializeField] private GameObject redLight;
    [SerializeField] private GameObject greenLight;

    [Header("Buhar")]
    [SerializeField] private ParticleSystem steamParticle;

    [Tooltip("Kaynama sırasında maksimum buhar miktarı")]
    [SerializeField] private float normalSteamRate = 8f;

    [Tooltip("Kaynama bittikten sonra buharın azalacağı süre")]
    [SerializeField] private float steamFadeDuration = 2f;


    private bool isHeating = false;
    private bool isHot = false;

    private Coroutine heatingCoroutine;
    private Coroutine steamFadeCoroutine;


    public bool IsHeating =>
        isHeating;

    public bool IsHot =>
        isHot;


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
        SetSteamRate(0f);


        if (steamParticle != null)
        {
            steamParticle.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }


        if (timerDisplay != null)
        {
            timerDisplay.HideTimer();
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (pickupItem == null)
            return;


        // =====================================================
        // YENİ SU GELDİYSE / SU SICAK DEĞİLSE
        // ESKİ SICAKLIK DURUMUNU TEMİZLE
        // =====================================================

        if (waterState != null &&
            !waterState.IsHot &&
            !isHeating)
        {
            isHot = false;

            // Yeni su geldiğinde yeşil yanmasın.
            SetGreenLight(false);
        }


        // =====================================================
        // KETTLE ELDEYSE
        // =====================================================

        if (pickupItem.IsHeld)
        {
            SetRedLight(false);
            SetGreenLight(false);


            // Elde iken buhar görünmez.
            if (steamParticle != null)
            {
                SetSteamRate(0f);

                steamParticle.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmitting
                );
            }


            return;
        }
    }


    // =========================================================
    // KETTLE DOCK KONTROLÜ
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


        // =====================================================
        // DOCK KONTROLÜ
        // =====================================================

        if (!IsKettleProperlyDocked())
        {
            Debug.Log(
                "Kettle standına tam oturmadan çalıştırılamaz."
            );

            return false;
        }


        // =====================================================
        // SU KONTROLÜ
        // =====================================================

        if (!waterState.HasWater)
        {
            Debug.Log(
                "Kettle'da su yok."
            );

            return false;
        }


        // =====================================================
        // EĞER SU ARTIK SICAK DEĞİLSE
        // ESKİ isHot DEĞERİNİ TEMİZLE
        // =====================================================

        if (!waterState.IsHot)
        {
            isHot = false;
        }


        // =====================================================
        // ZATEN ISINIYORSA
        // =====================================================

        if (isHeating)
            return false;


        // =====================================================
        // SU ZATEN SICAKSA
        // =====================================================

        if (isHot && waterState.IsHot)
        {
            Debug.Log(
                "Kettle'daki su zaten sıcak."
            );

            return false;
        }


        // =====================================================
        // ESKİ COROUTINE
        // =====================================================

        if (heatingCoroutine != null)
        {
            StopCoroutine(
                heatingCoroutine
            );

            heatingCoroutine = null;
        }


        if (steamFadeCoroutine != null)
        {
            StopCoroutine(
                steamFadeCoroutine
            );

            steamFadeCoroutine = null;
        }


        // =====================================================
        // KAYNATMAYI BAŞLAT
        // =====================================================

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


        // -----------------------------------------------------
        // BAŞLANGIÇ
        // -----------------------------------------------------

        SetRedLight(true);
        SetGreenLight(false);

        StartSteam();


        if (timerDisplay != null)
        {
            timerDisplay.ShowTimer(
                Mathf.CeilToInt(
                    heatDuration
                )
            );
        }


        Debug.Log(
            "Kettle ısınıyor..."
        );


        float elapsed = 0f;


        // -----------------------------------------------------
        // 10 SANİYELİK KAYNATMA
        // -----------------------------------------------------

        while (elapsed < heatDuration)
        {
            elapsed += Time.deltaTime;


            int remainingSeconds =
                Mathf.CeilToInt(
                    heatDuration - elapsed
                );


            remainingSeconds =
                Mathf.Clamp(
                    remainingSeconds,
                    0,
                    Mathf.CeilToInt(
                        heatDuration
                    )
                );


            if (timerDisplay != null &&
                remainingSeconds > 0)
            {
                timerDisplay.ShowTimer(
                    remainingSeconds
                );
            }


            yield return null;
        }


        // =====================================================
        // 10 SANİYE BİTTİ
        // KETTLE HEMEN HAZIR
        // =====================================================

        isHeating = false;
        isHot = true;


        // Suyu gerçekten sıcak yap
        waterState.SetHot();


        // Kırmızı hemen söner
        SetRedLight(false);


        // Sayaç hemen kaybolur
        if (timerDisplay != null)
        {
            timerDisplay.HideTimer();
        }


        // Yeşil hemen yanar
        SetGreenLight(true);


        Debug.Log(
            "Kettle hazır! Su sıcak."
        );


        // =====================================================
        // BUHAR 2 SANİYE DAHA AZALACAK
        // =====================================================

        steamFadeCoroutine =
            StartCoroutine(
                FadeOutSteam()
            );


        heatingCoroutine = null;
    }


    // =========================================================
    // BUHARIN 2 SANİYEDE AZALMASI
    // =========================================================

    private IEnumerator FadeOutSteam()
    {
        if (steamParticle == null)
            yield break;


        float elapsed = 0f;

        float startRate =
            normalSteamRate;


        SetSteamRate(
            startRate
        );


        while (
            elapsed < steamFadeDuration
        )
        {
            // Kettle oyuncunun elindeyse
            // buharı hemen kes.
            if (pickupItem != null &&
                pickupItem.IsHeld)
            {
                SetSteamRate(0f);

                steamParticle.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmitting
                );

                steamFadeCoroutine = null;

                yield break;
            }


            elapsed += Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed /
                    steamFadeDuration
                );


            float currentRate =
                Mathf.Lerp(
                    startRate,
                    0f,
                    t
                );


            SetSteamRate(
                currentRate
            );


            yield return null;
        }


        SetSteamRate(0f);


        steamParticle.Stop(
            true,
            ParticleSystemStopBehavior.StopEmitting
        );


        steamFadeCoroutine = null;
    }


    // =========================================================
    // BUHAR BAŞLAT
    // =========================================================

    private void StartSteam()
    {
        if (steamParticle == null)
            return;


        SetSteamRate(
            normalSteamRate
        );


        if (!steamParticle.isPlaying)
        {
            steamParticle.Play();
        }
    }


    // =========================================================
    // BUHARI DURDUR
    // =========================================================

    private void StopSteam()
    {
        if (steamParticle == null)
            return;


        SetSteamRate(0f);


        steamParticle.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );
    }


    // =========================================================
    // BUHAR MİKTARI
    // =========================================================

    private void SetSteamRate(float rate)
    {
        if (steamParticle == null)
            return;


        var emission =
            steamParticle.emission;


        emission.rateOverTime =
            rate;
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


        if (steamFadeCoroutine != null)
        {
            StopCoroutine(
                steamFadeCoroutine
            );

            steamFadeCoroutine = null;
        }


        isHeating = false;


        SetRedLight(false);
        StopSteam();


        if (timerDisplay != null)
        {
            timerDisplay.HideTimer();
        }


        // Eğer su henüz sıcak değilse
        // yeşil de kapalı kalır.
        if (waterState == null ||
            !waterState.IsHot)
        {
            isHot = false;
            SetGreenLight(false);
        }
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