using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ESC tuşuna basınca oyunu duraklatır: Time.timeScale = 0,
/// imleç serbest bırakılır, duraklatma paneli açılır.
///
/// ESC şu durumlarda pause menüsünü AÇMAZ (başka sistemler
/// kendi ESC davranışını yönetsin diye):
/// - Oyuncu bilgisayar ekranındaysa (ComputerInteraction.IsUsingComputer)
/// - Gün sonu özet paneli açıksa (EndOfDayUI.IsShowing)
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("Sahne")]
    [Tooltip("Ana menü sahnesinin Build Settings'teki tam adı.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Paneller")]
    [SerializeField] private GameObject pausePanelRoot;
    [SerializeField] private GameObject settingsPanelRoot;

    private bool isPaused = false;

    private void Awake()
    {
        if (pausePanelRoot != null)
        {
            pausePanelRoot.SetActive(false);
        }

        if (settingsPanelRoot != null)
        {
            settingsPanelRoot.SetActive(false);
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (IsBlockedByOtherSystem())
            return;

        // Ayarlar paneli açıksa ESC önce onu kapatsın,
        // pause menüsünü kapatmasın.
        if (settingsPanelRoot != null && settingsPanelRoot.activeSelf)
        {
            CloseSettings();
            return;
        }

        TogglePause();
    }

    private bool IsBlockedByOtherSystem()
    {
        if (ComputerInteraction.Instance != null &&
            ComputerInteraction.Instance.IsUsingComputer)
        {
            return true;
        }

        if (EndOfDayUI.IsShowing)
        {
            return true;
        }

        return false;
    }

    private void TogglePause()
    {
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    private void Pause()
    {
        isPaused = true;

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (pausePanelRoot != null)
        {
            pausePanelRoot.SetActive(true);
        }
    }

    public void Resume()
    {
        isPaused = false;

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (pausePanelRoot != null)
        {
            pausePanelRoot.SetActive(false);
        }

        if (settingsPanelRoot != null)
        {
            settingsPanelRoot.SetActive(false);
        }
    }

    // =========================================================
    // BUTON FONKSİYONLARI
    // =========================================================

    public void OnSettingsClicked()
    {
        if (settingsPanelRoot != null)
        {
            settingsPanelRoot.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        if (settingsPanelRoot != null)
        {
            settingsPanelRoot.SetActive(false);
        }
    }

    public void OnMainMenuClicked()
    {
        // Sahne değişmeden önce timeScale mutlaka 1'e dönmeli,
        // yoksa MainMenu sahnesi donmuş gibi açılır.
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}