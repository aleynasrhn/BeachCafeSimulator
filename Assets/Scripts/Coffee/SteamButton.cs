using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Makinedeki fiziksel buhar düğmesi.
///
/// Akış:
/// 1) Düğmeye E → buhar açılır (duman + ses). Sayaç BAŞLAMAZ.
/// 2) Oyuncu pitcher elindeyken SteamWandPoint'e E basılı tutar,
///    pitcher wand'ın altına yumuşakça gider.
/// 3) Pitcher yerine oturunca sayaç saymaya başlar:
///    - 3        → yeşil  (Latte sütü)
///    - 4 - 6    → turuncu
///    - 7 - 10   → kırmızı (yanık süt)
/// 4) Oyuncu pitcher'ı E ile alınca ısıtma durur ve süt ısısı kaydedilir.
///    10. saniyede buhar kendiliğinden kesilir.
/// </summary>
public class SteamButton : MonoBehaviour, IInteractable
{
    [Header("Referanslar")]
    [Tooltip("Buhar / duman efekti")]
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


    [Header("Boşta Kalma")]
    [Tooltip("Buhar açıldı ama pitcher yerleştirilmezse kaç saniye sonra kendiliğinden kapansın. 0 = kapanmaz.")]
    [SerializeField] private float idleAutoOffSeconds = 15f;


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

    // Buhar açık mı? (duman + ses)
    private bool isSteamOn = false;

    // Wand'ın altında bir pitcher var mı? (hareket, sayma, bekleme)
    private bool sessionActive = false;

    // Süt yandı, oyuncunun pitcher'ı almasını bekliyoruz.
    private bool waitingForPickup = false;

    private Coroutine idleRoutine;


    // =========================================================
    // SES
    // =========================================================

    private AudioSource runtimeAudioSource;


    // =========================================================
    // DIŞARIDAN OKUNANLAR (SteamWandPoint kullanır)
    // =========================================================

    public bool IsSteamOn =>
        isSteamOn;

    // Wand'ın altı dolu mu?
    public bool IsOccupied =>
        sessionActive;


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
    // PROMPT
    // =========================================================

    public string GetInteractPrompt()
    {
        if (sessionActive)
        {
            return waitingForPickup
                ? "Süt yandı! Pitcher'ı al"
                : "Buharlanıyor... (Pitcher'ı alınca durur)";
        }


        return isSteamOn
            ? "E - Buharı kapat"
            : "E - Buharı aç";
    }


    // =========================================================
    // INTERACT (DÜĞMEYE BASILDI)
    // =========================================================

    public void Interact(
        PlayerInteraction player)
    {
        // Pitcher wand'ın altındayken düğme devre dışı.
        // Durdurmak için pitcher alınır.
        if (sessionActive)
            return;


        if (isSteamOn)
        {
            TurnSteamOff();
        }
        else
        {
            TurnSteamOn();
        }
    }


    // =========================================================
    // BUHARI AÇ
    // =========================================================

    private void TurnSteamOn()
    {
        if (isSteamOn)
            return;

        isSteamOn = true;

        if (steamVisual != null)
            steamVisual.SetActive(true);

        StartSteamSound();

        if (idleAutoOffSeconds > 0f)
        {
            idleRoutine =
                StartCoroutine(
                    IdleAutoOffRoutine()
                );
        }
    }


    // =========================================================
    // BUHARI KAPAT
    // =========================================================

    private void TurnSteamOff()
    {
        isSteamOn = false;

        StopIdleRoutine();

        StopSteamSound();

        if (steamVisual != null)
            steamVisual.SetActive(false);
    }


    // =========================================================
    // BOŞTA KALAN BUHARI KAPAT
    // =========================================================

    private IEnumerator IdleAutoOffRoutine()
    {
        yield return new WaitForSeconds(
            idleAutoOffSeconds
        );

        idleRoutine = null;

        // Bu arada pitcher yerleştirildiyse dokunma.
        if (!sessionActive)
        {
            TurnSteamOff();
        }
    }


    private void StopIdleRoutine()
    {
        if (idleRoutine != null)
        {
            StopCoroutine(
                idleRoutine
            );

            idleRoutine = null;
        }
    }


    // =========================================================
    // ISITMAYI BAŞLAT (SteamWandPoint çağırır)
    // =========================================================

    // Pitcher wand'ın altına gönderilirken çağrılır.
    // Sayaç, pitcher yerine oturunca başlar.
    public void BeginHeating(
        MilkFiller filler,
        PickupItem pitcherItem)
    {
        if (sessionActive)
            return;

        if (!isSteamOn)
            return;

        if (filler == null ||
            pitcherItem == null)
        {
            return;
        }

        sessionActive = true;
        waitingForPickup = false;

        StopIdleRoutine();

        StartCoroutine(
            HeatingRoutine(
                filler,
                pitcherItem
            )
        );
    }


    // =========================================================
    // ISITMA ROUTINE
    // =========================================================

    private IEnumerator HeatingRoutine(
        MilkFiller filler,
        PickupItem pitcherItem)
    {
        // -----------------------------------------------------
        // PITCHER WAND'IN ALTINA GİDERKEN BEKLE
        // (SmoothDock sırasında item kilitli olur)
        // -----------------------------------------------------

        while (pitcherItem != null &&
               pitcherItem.IsInteractionLocked)
        {
            yield return null;
        }


        // Süt daha önce biraz ısıtıldıysa kaldığı yerden devam eder.
        float heat =
            filler.HeatSeconds;

        bool timedOut = false;


        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

        if (indicatorLight != null)
            indicatorLight.enabled = true;


        // -----------------------------------------------------
        // SAYAÇ
        // -----------------------------------------------------

        while (true)
        {
            // Oyuncu pitcher'ı aldı → dur.
            if (pitcherItem == null ||
                pitcherItem.IsHeld)
            {
                break;
            }

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


        // -----------------------------------------------------
        // ISIYI SÜTE KAYDET, BUHARI KES
        // -----------------------------------------------------

        filler.ApplyHeat(
            heat,
            frothedMilkMaterial,
            burntMilkMaterial
        );

        TurnSteamOff();


        // -----------------------------------------------------
        // 10'DA DURDUYSA: SAYAÇ KIRMIZI KALSIN,
        // OYUNCU PITCHER'I ALINCA KAPANSIN
        // -----------------------------------------------------

        if (timedOut)
        {
            waitingForPickup = true;

            UpdateCounter(
                MilkHeatRules.MaxSeconds
            );

            while (pitcherItem != null &&
                   !pitcherItem.IsHeld)
            {
                yield return null;
            }
        }


        // -----------------------------------------------------
        // TEMİZLE
        // -----------------------------------------------------

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        if (indicatorLight != null)
            indicatorLight.enabled = false;

        waitingForPickup = false;

        sessionActive = false;
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

        isSteamOn = false;
        sessionActive = false;
        waitingForPickup = false;
        idleRoutine = null;

        if (steamVisual != null)
            steamVisual.SetActive(false);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        if (indicatorLight != null)
            indicatorLight.enabled = false;
    }
}