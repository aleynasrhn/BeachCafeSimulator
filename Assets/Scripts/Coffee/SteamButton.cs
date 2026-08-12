using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Makinedeki fiziksel düğmeye eklenir.
/// Pitcher dock'ta süt varsa çalışır.
/// Buhar açılır, 3-2-1 geri sayar, sonra sütü köpürtür.
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

    private bool isSteaming = false;

    private void Start()
    {
        if (steamVisual != null)
            steamVisual.SetActive(false);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    public string GetInteractPrompt()
    {
        if (isSteaming)
            return "Buharlanıyor...";

        if (pitcherDock == null || !pitcherDock.IsOccupied)
            return "Önce pitcher'ı tak";

        MilkFiller filler = pitcherDock.DockedItem.GetComponent<MilkFiller>();

        if (filler == null || !filler.HasMilk)
            return "Önce süt doldur";

        return "E - Buharlandır";
    }

    public void Interact(PlayerInteraction player)
    {
        if (isSteaming)
            return;

        if (pitcherDock == null || !pitcherDock.IsOccupied)
            return;

        MilkFiller filler = pitcherDock.DockedItem.GetComponent<MilkFiller>();

        if (filler == null || !filler.HasMilk)
            return;

        StartCoroutine(SteamRoutine(filler));
    }

    private IEnumerator SteamRoutine(MilkFiller filler)
    {
        isSteaming = true;

        if (steamVisual != null)
            steamVisual.SetActive(true);

        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

        float timer = steamDuration;

        while (timer > 0f)
        {
            if (countdownText != null)
                countdownText.text = Mathf.CeilToInt(timer).ToString();

            timer -= Time.deltaTime;
            yield return null;
        }

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        if (steamVisual != null)
            steamVisual.SetActive(false);

        filler.SetFrothedMaterial(frothedMilkMaterial);

        isSteaming = false;
    }
}