using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Sahne Adı")]
    [Tooltip("Oyunun asıl oynanış sahnesinin adı (Build Settings'e eklenmiş olmalı).")]
    [SerializeField] private string gameSceneName = "BeachCafe";

    [Header("Butonlar")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Paneller")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Yeni Oyun Onayı (Kayıt Varsa Üzerine Yazılacağı İçin)")]
    [SerializeField] private GameObject newGameConfirmPanel;

    private void Awake()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
            continueButton.interactable = SaveManager.HasSave();
        }

        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(OnNewGameClicked);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (newGameConfirmPanel != null)
        {
            newGameConfirmPanel.SetActive(false);
        }
    }

    // =========================================================
    // KALDIĞIN YERDEN DEVAM ET
    // =========================================================

    private void OnContinueClicked()
    {
        if (!SaveManager.HasSave())
            return;

        SaveManager.PendingLoadFromSave = true;

        SceneManager.LoadScene(gameSceneName);
    }

    // =========================================================
    // YENİ OYUN
    // =========================================================

    private void OnNewGameClicked()
    {
        if (SaveManager.HasSave() && newGameConfirmPanel != null)
        {
            newGameConfirmPanel.SetActive(true);
            return;
        }

        StartNewGame();
    }

    // Yeni Oyun onay panelindeki "Evet, Üzerine Yaz" butonuna bağla.
    public void ConfirmNewGame()
    {
        if (newGameConfirmPanel != null)
        {
            newGameConfirmPanel.SetActive(false);
        }

        StartNewGame();
    }

    // Yeni Oyun onay panelindeki "Vazgeç" butonuna bağla.
    public void CancelNewGame()
    {
        if (newGameConfirmPanel != null)
        {
            newGameConfirmPanel.SetActive(false);
        }
    }

    private void StartNewGame()
    {
        SaveManager.DeleteSave();

        SaveManager.PendingLoadFromSave = false;

        SceneManager.LoadScene(gameSceneName);
    }

    // =========================================================
    // AYARLAR
    // =========================================================

    private void OnSettingsClicked()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    // Ayarlar panelindeki "Kapat" butonuna bağla.
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    // =========================================================
    // ÇIKIŞ
    // =========================================================

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}