using UnityEngine;

/// <summary>
/// Milk pitcher/frother objesine, PickupItem ve MilkFiller'ın YANINA eklenir.
///
/// Oyuncu elinde süt varken pitcher'a bakıp E'yi basılı tutunca:
/// - MilkFiller canlı olarak dolar.
/// - Süt dökme sesi çalar.
/// - E bırakılınca ses durur.
/// - Dolum tamamlanınca ses durur.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MilkPourInteraction : MonoBehaviour, IHoldInteractable
{
    [Header("E Basılı Tutma")]
    [SerializeField] private float holdDuration = 2f;

    [SerializeField] private string acceptedItemName = "Milk";

    [Tooltip(
        "Aynı objedeki veya başka bir yerdeki MilkFiller component'ini buraya sürükle."
    )]
    [SerializeField] private MilkFiller milkFiller;

    [Header("Süt Dökme Sesi")]
    [SerializeField] private AudioClip milkPourClip;

    [SerializeField]
    [Range(0f, 1f)]
    private float milkPourVolume = 1f;

    private AudioSource runtimeAudioSource;

    private bool milkSoundPlaying = false;


    // =========================================================
    // START
    // =========================================================

    private void Awake()
    {
        CreateAudioSource();
    }


    // =========================================================
    // AUDIO SOURCE OLUŞTUR
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
            milkPourVolume;

        runtimeAudioSource.mute = false;

        runtimeAudioSource.priority = 128;
    }


    // =========================================================
    // HOLD
    // =========================================================

    public float HoldDuration =>
        holdDuration;


    public string GetHoldPrompt()
    {
        return "E'ye basılı tut";
    }


    // =========================================================
    // BAŞLAYABİLİR Mİ?
    // =========================================================

    public bool CanStartHold(
        PlayerInteraction player
    )
    {
        if (player == null)
            return false;

        PickupItem held =
            player.GetHeldItem();

        if (held == null)
            return false;

        if (held.ItemName != acceptedItemName)
            return false;

        if (milkFiller == null)
            return false;

        if (milkFiller.IsFull)
            return false;

        return true;
    }


    // =========================================================
    // E BASILIYKEN
    // =========================================================

    public void OnHoldProgress(
        PlayerInteraction player,
        float progress01
    )
    {
        if (player == null)
            return;

        if (milkFiller == null)
            return;

        // Süt dökme sesi
        StartMilkPourSound();

        // Pitcher dolumu
        milkFiller.SetFillProgress(
            progress01
        );
    }


    // =========================================================
    // DÖKME TAMAMLANDI
    // =========================================================

    public void OnHoldComplete(
        PlayerInteraction player
    )
    {
        if (milkFiller == null)
        {
            StopMilkPourSound();
            return;
        }

        milkFiller.SetFillProgress(1f);

        milkFiller.CompleteFill();

        StopMilkPourSound();
    }


    // =========================================================
    // SESİ BAŞLAT
    // =========================================================

    private void StartMilkPourSound()
    {
        if (milkPourClip == null)
        {
            Debug.LogError(
                "MILK PITCHER POUR CLIP BOS! " +
                gameObject.name
            );

            return;
        }

        if (runtimeAudioSource == null)
        {
            CreateAudioSource();
        }

        if (milkSoundPlaying)
            return;

        runtimeAudioSource.clip =
            milkPourClip;

        runtimeAudioSource.volume =
            milkPourVolume;

        runtimeAudioSource.loop = true;

        runtimeAudioSource.spatialBlend = 0f;

        runtimeAudioSource.mute = false;

        runtimeAudioSource.Play();

        milkSoundPlaying = true;

        Debug.Log(
            "MILK PITCHER SUT DOKME SESI BASLADI!"
        );
    }


    // =========================================================
    // SESİ DURDUR
    // =========================================================

    private void StopMilkPourSound()
    {
        milkSoundPlaying = false;

        if (runtimeAudioSource == null)
            return;

        if (runtimeAudioSource.isPlaying)
        {
            runtimeAudioSource.Stop();

            Debug.Log(
                "MILK PITCHER SUT DOKME SESI DURDU!"
            );
        }
    }


    // =========================================================
    // HOLD İPTAL
    // =========================================================

    public void CancelHold()
    {
        StopMilkPourSound();
    }


    // =========================================================
    // OBJECT DISABLE
    // =========================================================

    private void OnDisable()
    {
        StopMilkPourSound();
    }
}