using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Makinedeki fiziksel düğmeye eklenir.
/// Pitcher dock'ta süt varsa çalışır.
///
/// Tuşa basınca sayaç 0'dan yukarı sayar:
/// - 3        → yeşil  (Latte sütü)
/// - 4 - 6    → turuncu (6 = Cappuccino sütü)
/// - 7 - 10   → kırmızı (yanık süt)
/// - 10'da buhar kendiliğinden durur.
///
/// Oyuncu pitcher'ı makineden alınca buhar durur
/// ve sütün ısısı kaydedilir.
/// </summary>
public class SteamButton : MonoBehaviour, IInteractable
{
    [Header("Referanslar")]
    [Tooltip("Steam wand altındaki MachineDockPoint")]
    [SerializeField] private MachineDockPoint pitcherDock;

    [Tooltip("Buhar efekti")]
    [SerializeField] private GameObject steamVisual;

    [Tooltip("Isıtılmış süt materyali")]
    [SerializeField] private Material frothedMilkMaterial;

    [Tooltip("Yanmış süt materyali (boş bırakılabilir)")]
    [SerializeField] private Material burntMilkMaterial;


    [Header("Sayaç ve Işık")]
    [SerializeField] private TMP_Text countdownText;

    [Tooltip("Makine üzerindeki gösterge ışığı (boş bırakılabilir)")]
    [SerializeField] private Light indicatorLight;

    [SerializeField] private Color coldColor = Color.white;
    [SerializeField] private Color greenColor = Color.green;
    [SerializeField] private Color orangeColor = new Color(1f, 0.55f, 0f);
    [SerializeField] private Color redColor = Color.red;


    // =========================================================
    // BUHAR SESİ
    // =========================================================

    [Header("Buhar Sesi")]
    [SerializeField] private AudioClip steamSoundClip;

    [SerializeField]
    [Range(0f, 1f)]
    private float steamSoundVolume = 1f;

    [Tooltip("Süre dolup buhar kendiliğinden bitmeden kaç saniye önce ses azalmaya başlasın.")]
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

        if (indicatorLight != null)
            indicatorLight.enabled = false;

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
    // DOCK'TAKİ PITCHER'I BUL
    // =========================================================

    private MilkFiller GetDockedFiller()
    {
        if (pitcherDock == null ||
            !pitcherDock.IsOccupied ||
            pitcherDock.DockedItem == null)
        {
            return null;
        }

        return pitcherDock.DockedItem
            .GetComponent<MilkFiller>();
    }


    // =========================================================
    // PITCHER HÂLÂ DOCK'TA MI?
    // =========================================================

    private bool IsStillDocked(
        MilkFiller filler,
        PickupItem pitcherItem)
    {
        // Oyuncu eline aldıysa dock'tan ayrılmıştır.
        if (pitcherItem != null &&
            pitcherItem.IsHeld)
        {
            return false;
        }

        return GetDockedFiller() == filler;
    }


    // =========================================================
    // PROMPT
    // =========================================================

    public string GetInteractPrompt()
    {
        if (isSteaming)
            return "Buharlanıyor... (Pitcher'ı alınca durur)";


        MilkFiller filler =
            GetDockedFiller();


        if (filler == null)
            return "Önce pitcher'ı tak";


        if (!filler.HasMilk)
            return "Önce süt doldur";


        if (filler.IsBurnt)
            return "Süt yandı!";


        if (filler.IsFrothed)
            return "E - Isıtmaya devam et";


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


        MilkFiller filler =
            GetDockedFiller();


        if (filler == null ||
            !filler.HasMilk)
        {
            return;
        }


        // Yanık süt tekrar ısıtılamaz.
        if (filler.IsBurnt)
            return;


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

        PickupItem pitcherItem =
            filler.GetComponent<PickupItem>();

        // Süt daha önce biraz ısıtıldıysa kaldığı yerden devam eder.
        float heat =
            filler.HeatSeconds;

        bool timedOut = false;


        if (steamVisual != null)
            steamVisual.SetActive(true);

        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

        if (indicatorLight != null)
            indicatorLight.enabled = true;

        StartSteamSound();


        while (true)
        {
            // Oyuncu pitcher'ı aldı → dur.
            if (!IsStillDocked(filler, pitcherItem))
                break;

            // 10 saniyeye ulaştı → kendiliğinden dur.
            if (heat >= MilkHeatRules.MaxSeconds)
            {
                heat = MilkHeatRules.MaxSeconds;
                timedOut = true;
                break;
            }

            UpdateCounter(heat);

            UpdateSteamSoundFade(
                MilkHeatRules.MaxSeconds - heat
            );

            heat += Time.deltaTime;

            yield return null;
        }


        heat = Mathf.Min(
            heat,
            MilkHeatRules.MaxSeconds
        );


        StopSteamSound();

        if (steamVisual != null)
            steamVisual.SetActive(false);


        // Isıyı süte kaydet.
        filler.ApplyHeat(
            heat,
            frothedMilkMaterial,
            burntMilkMaterial
        );

        isSteaming = false;


        if (timedOut)
        {
            // Sayaç 10'da kırmızı kalsın,
            // oyuncu pitcher'ı alınca kapansın.
            UpdateCounter(
                MilkHeatRules.MaxSeconds
            );

            while (IsStillDocked(filler, pitcherItem))
            {
                yield return null;
            }
        }


        // Arada yeni bir buharlama başlamadıysa kapat.
        if (!isSteaming)
        {
            if (countdownText != null)
                countdownText.gameObject.SetActive(false);

            if (indicatorLight != null)
                indicatorLight.enabled = false;
        }
    }


    // =========================================================
    // SAYAÇ VE IŞIK GÜNCELLE
    // =========================================================

    private void UpdateCounter(float heat)
    {
        Color color =
            GetHeatColor(heat);

        if (countdownText != null)
        {
            countdownText.text =
                Mathf.FloorToInt(heat).ToString();

            countdownText.color =
                color;
        }

        if (indicatorLight != null)
        {
            indicatorLight.color =
                color;
        }
    }


    private Color GetHeatColor(float heat)
    {
        if (heat < MilkHeatRules.LatteMin)
            return coldColor;

        if (heat < MilkHeatRules.LatteMax)
            return greenColor;

        if (heat < MilkHeatRules.BurntMin)
            return orangeColor;

        return redColor;
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


        if (steamFadeOutDuration <= 0f ||
            remainingTime > steamFadeOutDuration)
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


        runtimeAudioSource.volume =
            Mathf.Lerp(
                steamSoundVolume,
                0f,
                fadeProgress
            );
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

        isSteaming = false;

        if (steamVisual != null)
            steamVisual.SetActive(false);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        if (indicatorLight != null)
            indicatorLight.enabled = false;
    }
}