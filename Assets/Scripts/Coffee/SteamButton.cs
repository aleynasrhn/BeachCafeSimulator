using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Makinedeki fiziksel düğmeye eklenir.
/// Pitcher dock'ta süt varsa çalışır.
/// Buhar açılır, 3-2-1 geri sayar, sonra sütü köpürtür.
///
/// Buhar sesi:
/// - Buhar başlarken çalar.
/// - Buharlama boyunca devam eder.
/// - Son kısımda yavaşça azalır.
/// - Tamamen bitince ses kapanır.
/// </summary>
public class SteamButton : MonoBehaviour, IInteractable
{
    [Header("Referanslar")]
    [Tooltip("Steam wand altındaki MachineDockPoint")]
    [SerializeField] private MachineDockPoint pitcherDock;

    [Tooltip("Buhar efekti")]
    [SerializeField] private GameObject steamVisual;

    [Tooltip("Köpürmüş süt materyali")]
    [SerializeField] private Material frothedMilkMaterial;


    [Header("Sayaç")]
    [SerializeField] private TMP_Text countdownText;


    [Header("Ayarlar")]
    [SerializeField] private float steamDuration = 3f;


    // =========================================================
    // BUHAR SESİ
    // =========================================================

    [Header("Buhar Sesi")]
    [SerializeField] private AudioClip steamSoundClip;

    [SerializeField]
    [Range(0f, 1f)]
    private float steamSoundVolume = 1f;

    [Tooltip("Buhar bitmeden kaç saniye önce ses azalmaya başlasın.")]
    [SerializeField] private float steamFadeOutDuration = 0.8f;


    // =========================================================
    // DURUM
    // =========================================================

    private bool isSteaming = false;


    // =========================================================
    // SES
    // =========================================================

    private AudioSource runtimeAudioSource;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (steamVisual != null)
            steamVisual.SetActive(false);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        CreateAudioSource();
    }


    // =========================================================
    // AUDIO SOURCE
    // =========================================================

    private void CreateAudioSource()
    {
        runtimeAudioSource =
            gameObject.AddComponent<AudioSource>();

        runtimeAudioSource.playOnAwake = false;
        runtimeAudioSource.loop = true;

        // 2D ses
        runtimeAudioSource.spatialBlend = 0f;

        runtimeAudioSource.volume =
            steamSoundVolume;

        runtimeAudioSource.mute = false;
        runtimeAudioSource.priority = 128;
    }


    // =========================================================
    // PROMPT
    // =========================================================

    public string GetInteractPrompt()
    {
        if (isSteaming)
            return "Buharlanıyor...";


        if (pitcherDock == null ||
            !pitcherDock.IsOccupied)
        {
            return "Önce pitcher'ı tak";
        }


        if (pitcherDock.DockedItem == null)
            return "Önce pitcher'ı tak";


        MilkFiller filler =
            pitcherDock.DockedItem.GetComponent<MilkFiller>();


        if (filler == null ||
            !filler.HasMilk)
        {
            return "Önce süt doldur";
        }


        return "E - Buharlandır";
    }


    // =========================================================
    // INTERACT
    // =========================================================

    public void Interact(
        PlayerInteraction player)
    {
        if (isSteaming)
            return;


        if (pitcherDock == null ||
            !pitcherDock.IsOccupied)
        {
            return;
        }


        if (pitcherDock.DockedItem == null)
            return;


        MilkFiller filler =
            pitcherDock.DockedItem.GetComponent<MilkFiller>();


        if (filler == null ||
            !filler.HasMilk)
        {
            return;
        }


        StartCoroutine(
            SteamRoutine(
                filler
            )
        );
    }


    // =========================================================
    // STEAM ROUTINE
    // =========================================================

    private IEnumerator SteamRoutine(
        MilkFiller filler)
    {
        isSteaming = true;


        // =====================================================
        // BUHAR GÖRSELİNİ AÇ
        // =====================================================

        if (steamVisual != null)
            steamVisual.SetActive(true);


        // =====================================================
        // GERİ SAYIMI AÇ
        // =====================================================

        if (countdownText != null)
            countdownText.gameObject.SetActive(true);


        // =====================================================
        // SESİ BAŞLAT
        // =====================================================

        StartSteamSound();


        float timer = steamDuration;


        while (timer > 0f)
        {
            // -------------------------------------------------
            // GERİ SAYIM
            // -------------------------------------------------

            if (countdownText != null)
            {
                countdownText.text =
                    Mathf.CeilToInt(
                        timer
                    ).ToString();
            }


            // -------------------------------------------------
            // SES FADE OUT
            // -------------------------------------------------

            UpdateSteamSoundFade(
                timer
            );


            timer -=
                Time.deltaTime;


            yield return null;
        }


        // =====================================================
        // SESİ TAMAMEN KAPAT
        // =====================================================

        StopSteamSound();


        // =====================================================
        // GERİ SAYIMI KAPAT
        // =====================================================

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);


        // =====================================================
        // BUHAR GÖRSELİNİ KAPAT
        // =====================================================

        if (steamVisual != null)
            steamVisual.SetActive(false);


        // =====================================================
        // SÜTÜ KÖPÜRT
        // =====================================================

        filler.SetFrothedMaterial(
            frothedMilkMaterial
        );


        isSteaming = false;
    }


    // =========================================================
    // SES FADE KONTROLÜ
    // =========================================================

    private void UpdateSteamSoundFade(
        float remainingTime
    )
    {
        if (runtimeAudioSource == null)
            return;

        if (!runtimeAudioSource.isPlaying)
            return;


        // Fade süresi 0 veya negatifse
        // ses sonuna kadar aynı seviyede kalır.
        if (steamFadeOutDuration <= 0f)
        {
            runtimeAudioSource.volume =
                steamSoundVolume;

            return;
        }


        // Fade henüz başlamadı.
        if (remainingTime > steamFadeOutDuration)
        {
            runtimeAudioSource.volume =
                steamSoundVolume;

            return;
        }


        // 0 = fade başlangıcı
        // 1 = tamamen sessiz
        float fadeProgress =
            Mathf.Clamp01(
                1f -
                (
                    remainingTime /
                    steamFadeOutDuration
                )
            );


        float currentVolume =
            Mathf.Lerp(
                steamSoundVolume,
                0f,
                fadeProgress
            );


        runtimeAudioSource.volume =
            currentVolume;
    }


    // =========================================================
    // BUHAR SESİNİ BAŞLAT
    // =========================================================

    private void StartSteamSound()
    {
        if (steamSoundClip == null)
        {
            Debug.LogError(
                "STEAM SOUND CLIP BOS! " +
                gameObject.name
            );

            return;
        }


        if (runtimeAudioSource == null)
        {
            CreateAudioSource();
        }


        if (runtimeAudioSource.isPlaying)
            return;


        runtimeAudioSource.clip =
            steamSoundClip;

        runtimeAudioSource.volume =
            steamSoundVolume;

        runtimeAudioSource.loop = true;

        runtimeAudioSource.spatialBlend = 0f;

        runtimeAudioSource.mute = false;


        runtimeAudioSource.Play();


        Debug.Log(
            "BUHAR SESİ BAŞLADI!"
        );
    }


    // =========================================================
    // BUHAR SESİNİ DURDUR
    // =========================================================

    private void StopSteamSound()
    {
        if (runtimeAudioSource == null)
            return;


        runtimeAudioSource.volume =
            0f;


        if (runtimeAudioSource.isPlaying)
        {
            runtimeAudioSource.Stop();

            Debug.Log(
                "BUHAR SESİ DURDU!"
            );
        }


        // Bir sonraki kullanımda tekrar
        // normal volume'den başlasın.
        runtimeAudioSource.volume =
            steamSoundVolume;
    }


    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        StopSteamSound();
    }
}