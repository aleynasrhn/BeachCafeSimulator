using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Ayarlar panelindeki tüm kontrolleri PlayerPrefs ile kalıcı
/// yapar ve gerçek sistem ayarlarına uygular.
///
/// KURULUM:
/// - Her alanı ilgili UI objesine sürükle.
/// - Ses sliderlarının Min/Max Value'sini 0-1 bırak (Unity varsayılanı).
/// - Mouse Hassasiyeti sliderının Min/Max değerini kendi istediğin
///   aralığa ayarla (örn. 50-400) — kod bu aralığı doğrudan kullanır.
/// - Audio Mixer opsiyonel: atarsan Müzik/Efekt sesleri gerçekten
///   kısılır; atamazsan sadece Ana Ses (tüm sesler, AudioListener
///   üzerinden) kontrol edilir, diğer ikisi yalnızca tercih olarak
///   kaydedilir — ileride mixer eklediğinde otomatik devreye girer.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    [Header("Ses")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Ses - Mixer (Opsiyonel)")]
    [Tooltip("Atanırsa Müzik/Efekt sesleri bu mixer üzerinden kısılır.")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [SerializeField] private string sfxVolumeParam = "SFXVolume";

    [Header("Görüntü")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("Oyun")]
    [Tooltip("Min/Max değerlerini Inspector'da (Slider component'inde) istediğin aralığa ayarla, örn. 50-400.")]
    [SerializeField] private Slider mouseSensitivitySlider;

    private Resolution[] availableResolutions;

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";
    private const string QualityLevelKey = "QualityLevel";
    private const string ResolutionIndexKey = "ResolutionIndex";
    private const string MouseSensitivityKey = "MouseSensitivity";

    private void Start()
    {
        SetupQualityDropdown();
        SetupResolutionDropdown();
        LoadAndApplySavedSettings();
        AddListeners();
    }

    // =========================================================
    // DROPDOWN KURULUMU
    // =========================================================

    private void SetupQualityDropdown()
    {
        if (qualityDropdown == null)
            return;

        qualityDropdown.ClearOptions();

        List<string> names = new List<string>(QualitySettings.names);

        qualityDropdown.AddOptions(names);
    }

    private void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null)
            return;

        Resolution[] rawResolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        HashSet<string> seen = new HashSet<string>();

        List<Resolution> uniqueResolutions = new List<Resolution>();

        foreach (Resolution res in rawResolutions)
        {
            string label = $"{res.width} x {res.height}";

            if (seen.Contains(label))
                continue;

            seen.Add(label);

            uniqueResolutions.Add(res);

            options.Add(label);
        }

        availableResolutions = uniqueResolutions.ToArray();

        resolutionDropdown.AddOptions(options);
    }

    // =========================================================
    // KAYITLI AYARLARI YÜKLE VE UYGULA
    // =========================================================

    private void LoadAndApplySavedSettings()
    {
        // ---------------------------------------------
        // SES
        // ---------------------------------------------

        float masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        float sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);

        if (masterVolumeSlider != null)
            masterVolumeSlider.SetValueWithoutNotify(masterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(musicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);

        ApplyMasterVolume(masterVolume);
        ApplyMusicVolume(musicVolume);
        ApplySfxVolume(sfxVolume);

        // ---------------------------------------------
        // GRAFİK KALİTESİ
        // ---------------------------------------------

        int savedQuality =
            PlayerPrefs.GetInt(QualityLevelKey, QualitySettings.GetQualityLevel());

        if (qualityDropdown != null)
        {
            qualityDropdown.SetValueWithoutNotify(savedQuality);
        }

        QualitySettings.SetQualityLevel(savedQuality, true);

        // ---------------------------------------------
        // ÇÖZÜNÜRLÜK
        // ---------------------------------------------

        if (resolutionDropdown != null && availableResolutions != null)
        {
            int savedResolutionIndex = PlayerPrefs.GetInt(ResolutionIndexKey, -1);

            int indexToApply = savedResolutionIndex;

            if (indexToApply < 0 || indexToApply >= availableResolutions.Length)
            {
                indexToApply = FindCurrentResolutionIndex();
            }

            if (indexToApply >= 0)
            {
                resolutionDropdown.SetValueWithoutNotify(indexToApply);

                ApplyResolution(indexToApply);
            }
        }

        // ---------------------------------------------
        // MOUSE HASSASİYETİ
        // ---------------------------------------------

        if (mouseSensitivitySlider != null)
        {
            float defaultSensitivity =
                (mouseSensitivitySlider.minValue + mouseSensitivitySlider.maxValue) / 2f;

            float savedSensitivity =
                PlayerPrefs.GetFloat(MouseSensitivityKey, defaultSensitivity);

            mouseSensitivitySlider.SetValueWithoutNotify(savedSensitivity);

            PlayerPrefs.SetFloat(MouseSensitivityKey, savedSensitivity);
        }
    }

    private int FindCurrentResolutionIndex()
    {
        for (int i = 0; i < availableResolutions.Length; i++)
        {
            if (availableResolutions[i].width == Screen.currentResolution.width &&
                availableResolutions[i].height == Screen.currentResolution.height)
            {
                return i;
            }
        }

        return -1;
    }

    // =========================================================
    // LISTENER'LAR
    // =========================================================

    private void AddListeners()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
    }

    private void OnDestroy()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);

        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);
    }

    // =========================================================
    // SES UYGULAMA
    // =========================================================

    private void OnMasterVolumeChanged(float value)
    {
        ApplyMasterVolume(value);

        PlayerPrefs.SetFloat(MasterVolumeKey, value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        ApplyMusicVolume(value);

        PlayerPrefs.SetFloat(MusicVolumeKey, value);
    }

    private void OnSfxVolumeChanged(float value)
    {
        ApplySfxVolume(value);

        PlayerPrefs.SetFloat(SfxVolumeKey, value);
    }

    private void ApplyMasterVolume(float value)
    {
        AudioListener.volume = value;
    }

    private void ApplyMusicVolume(float value)
    {
        if (audioMixer == null)
            return;

        float db = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;

        audioMixer.SetFloat(musicVolumeParam, db);
    }

    private void ApplySfxVolume(float value)
    {
        if (audioMixer == null)
            return;

        float db = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;

        audioMixer.SetFloat(sfxVolumeParam, db);
    }

    // =========================================================
    // GRAFİK KALİTESİ
    // =========================================================

    private void OnQualityChanged(int index)
    {
        QualitySettings.SetQualityLevel(index, true);

        PlayerPrefs.SetInt(QualityLevelKey, index);
    }

    // =========================================================
    // ÇÖZÜNÜRLÜK
    // =========================================================

    private void OnResolutionChanged(int index)
    {
        ApplyResolution(index);

        PlayerPrefs.SetInt(ResolutionIndexKey, index);
    }

    private void ApplyResolution(int index)
    {
        if (availableResolutions == null ||
            index < 0 ||
            index >= availableResolutions.Length)
        {
            return;
        }

        Resolution res = availableResolutions[index];

        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
    }

    // =========================================================
    // MOUSE HASSASİYETİ
    // =========================================================

    private void OnMouseSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat(MouseSensitivityKey, value);

        PlayerMovement player = FindObjectOfType<PlayerMovement>();

        if (player != null)
        {
            player.mouseSensitivity = value;
        }
    }
}