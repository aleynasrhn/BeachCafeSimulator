using System.Collections;
using UnityEngine;

/// <summary>
/// Tam ekranı kaplayan siyah bir CanvasGroup üzerinden
/// fade-in/fade-out sağlar. Time.timeScale = 0 olsa bile
/// (gün sonu paneli açıkken) düzgün çalışsın diye
/// unscaledDeltaTime kullanır.
/// </summary>
public class FadeScreenUI : MonoBehaviour
{
    public static FadeScreenUI Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    // =========================================================
    // SİYAHA GEÇ (0 → 1)
    // =========================================================

    public IEnumerator FadeOut(float duration)
    {
        yield return Fade(0f, 1f, duration);

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }
    }

    // =========================================================
    // SİYAHTAN AÇIL (1 → 0)
    // =========================================================

    public IEnumerator FadeIn(float duration)
    {
        yield return Fade(1f, 0f, duration);

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (canvasGroup == null)
            yield break;

        float t = 0f;

        float safeDuration = Mathf.Max(0.01f, duration);

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / safeDuration;

            canvasGroup.alpha =
                Mathf.Lerp(from, to, Mathf.Clamp01(t));

            yield return null;
        }

        canvasGroup.alpha = to;
    }
}