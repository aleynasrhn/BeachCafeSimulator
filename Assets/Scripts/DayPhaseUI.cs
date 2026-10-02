using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Ekranın altında görünen üç durumu yönetir:
/// - Gün başlamadan: "Günü Başlat (Space)"
/// - 20:00'den sonra: "Kafe kapandı..." banner + "Günü Bitir (Space)"
/// - Space'e basıp hâlâ aktif müşteri varsa: geçici uyarı
/// </summary>
public class DayPhaseUI : MonoBehaviour
{
    [Header("Gün Başlamadan Önce")]
    [SerializeField] private GameObject startPromptRoot;

    [Header("Kafe Kapandı (20:00) Mesajı")]
    [SerializeField] private GameObject closedBannerRoot;

    [Header("Günü Bitir Uyarısı (Kapandıktan Sonra)")]
    [SerializeField] private GameObject endDayPromptRoot;

    [Header("Bitirilemez Uyarısı (Geçici)")]
    [SerializeField] private GameObject cannotEndRoot;
    [SerializeField] private TMP_Text cannotEndText;
    [SerializeField] private float cannotEndMessageDuration = 2.5f;

    private Coroutine cannotEndCoroutine;

    private void OnEnable()
    {
        if (DayCycleManager.Instance == null)
            return;

        DayCycleManager.Instance.OnDayStarted += HandleDayStarted;
        DayCycleManager.Instance.OnNoMoreCustomers += HandleNoMoreCustomers;
        DayCycleManager.Instance.OnCannotEndDay += HandleCannotEndDay;
        DayCycleManager.Instance.OnDayEnded += HandleDayEnded;
    }

    private void OnDisable()
    {
        if (DayCycleManager.Instance == null)
            return;

        DayCycleManager.Instance.OnDayStarted -= HandleDayStarted;
        DayCycleManager.Instance.OnNoMoreCustomers -= HandleNoMoreCustomers;
        DayCycleManager.Instance.OnCannotEndDay -= HandleCannotEndDay;
        DayCycleManager.Instance.OnDayEnded -= HandleDayEnded;
    }

    private void Start()
    {
        bool notStarted =
            DayCycleManager.Instance == null ||
            DayCycleManager.Instance.CurrentPhase ==
            DayCycleManager.DayPhase.NotStarted;

        SetRootActive(startPromptRoot, notStarted);
        SetRootActive(closedBannerRoot, false);
        SetRootActive(endDayPromptRoot, false);
        SetRootActive(cannotEndRoot, false);
    }

    private void HandleDayStarted()
    {
        SetRootActive(startPromptRoot, false);
        SetRootActive(closedBannerRoot, false);
        SetRootActive(endDayPromptRoot, false);
    }

    private void HandleNoMoreCustomers()
    {
        SetRootActive(closedBannerRoot, true);
        SetRootActive(endDayPromptRoot, true);
    }

    private void HandleCannotEndDay()
    {
        if (cannotEndRoot == null)
            return;

        if (cannotEndCoroutine != null)
        {
            StopCoroutine(cannotEndCoroutine);
        }

        cannotEndCoroutine = StartCoroutine(ShowCannotEndMessage());
    }

    private IEnumerator ShowCannotEndMessage()
    {
        cannotEndRoot.SetActive(true);

        if (cannotEndText != null)
        {
            cannotEndText.text =
                "Günü henüz bitiremezsin. Bekleyen müşterilerin işlemlerini tamamla.";
        }

        yield return new WaitForSeconds(cannotEndMessageDuration);

        cannotEndRoot.SetActive(false);

        cannotEndCoroutine = null;
    }

    private void HandleDayEnded(DayResult result)
    {
        SetRootActive(closedBannerRoot, false);
        SetRootActive(endDayPromptRoot, false);
    }

    private void SetRootActive(GameObject root, bool active)
    {
        if (root != null)
        {
            root.SetActive(active);
        }
    }
}