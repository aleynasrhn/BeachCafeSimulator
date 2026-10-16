using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ana kahve butonlarının bulunduğu satırı oklarla kaydırır.
///
/// Ekranda "visibleCount" kadar buton görünür (örn. 4).
/// Sağ ok satırı bir buton kadar sola kaydırır:
/// Americano çıkar, sağdan yeni kahve girer.
///
/// Kurulum: bu script Viewport objesine eklenir
/// (içinde RectMask2D olan obje).
/// </summary>
public class CoffeePagerUI : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Kahve butonlarının çocuk olduğu, Grid/Horizontal Layout Group'lu obje.")]
    [SerializeField] private RectTransform content;

    [SerializeField] private Button leftArrow;

    [SerializeField] private Button rightArrow;


    [Header("Ayarlar")]
    [Tooltip("Aynı anda ekranda kaç kahve butonu görünsün.")]
    [SerializeField] private int visibleCount = 4;

    [Tooltip("Kayma hızı. Büyüdükçe daha hızlı oturur.")]
    [SerializeField] private float slideSpeed = 12f;


    // =========================================================
    // DURUM
    // =========================================================

    // Soldan kaçıncı buton ilk görünen? (0 = Americano)
    private int pageIndex = 0;

    // İki butonun merkezleri arasındaki mesafe.
    private float step = 0f;

    private float startX = 0f;

    private float targetX = 0f;


    // En fazla kaç adım sağa kaydırılabilir.
    private int MaxIndex =>
        content != null
            ? Mathf.Max(0, content.childCount - visibleCount)
            : 0;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (content == null)
        {
            Debug.LogError(
                $"{gameObject.name}: Content atanmadı!"
            );

            enabled = false;

            return;
        }


        // Layout Group butonları yerleştirsin.
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            content
        );


        startX =
            content.anchoredPosition.x;

        targetX =
            startX;

        step =
            CalculateStep();


        if (leftArrow != null)
        {
            leftArrow.onClick.AddListener(
                GoLeft
            );
        }

        if (rightArrow != null)
        {
            rightArrow.onClick.AddListener(
                GoRight
            );
        }


        UpdateArrows();
    }


    // =========================================================
    // ADIM MESAFESİ
    // =========================================================

    private float CalculateStep()
    {
        if (content.childCount < 2)
            return 0f;

        RectTransform first =
            content.GetChild(0) as RectTransform;

        RectTransform second =
            content.GetChild(1) as RectTransform;

        if (first == null ||
            second == null)
        {
            return 0f;
        }

        return Mathf.Abs(
            second.localPosition.x -
            first.localPosition.x
        );
    }


    // =========================================================
    // OKLAR
    // =========================================================

    public void GoRight()
    {
        if (pageIndex >= MaxIndex)
            return;

        pageIndex++;

        UpdateTarget();
    }


    public void GoLeft()
    {
        if (pageIndex <= 0)
            return;

        pageIndex--;

        UpdateTarget();
    }


    // Sipariş ekranı sıfırlanırken çağrılabilir.
    public void ResetToFirstPage()
    {
        pageIndex = 0;

        targetX = startX;

        if (content != null)
        {
            content.anchoredPosition =
                new Vector2(
                    startX,
                    content.anchoredPosition.y
                );
        }

        UpdateArrows();
    }


    private void UpdateTarget()
    {
        // Satır sola kayar: pageIndex arttıkça x azalır.
        targetX =
            startX - pageIndex * step;

        UpdateArrows();
    }


    private void UpdateArrows()
    {
        if (leftArrow != null)
        {
            leftArrow.interactable =
                pageIndex > 0;
        }

        if (rightArrow != null)
        {
            rightArrow.interactable =
                pageIndex < MaxIndex;
        }
    }


    // =========================================================
    // YUMUŞAK KAYMA
    // =========================================================

    private void Update()
    {
        if (content == null)
            return;


        float currentX =
            content.anchoredPosition.x;


        if (Mathf.Abs(currentX - targetX) < 0.01f)
            return;


        // Time.unscaledDeltaTime: oyun duraklatılsa bile
        // arayüz kaymaya devam eder.
        float t =
            1f - Mathf.Exp(
                -slideSpeed *
                Time.unscaledDeltaTime
            );


        float newX =
            Mathf.Lerp(
                currentX,
                targetX,
                t
            );


        content.anchoredPosition =
            new Vector2(
                newX,
                content.anchoredPosition.y
            );
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (leftArrow != null)
        {
            leftArrow.onClick.RemoveListener(
                GoLeft
            );
        }

        if (rightArrow != null)
        {
            rightArrow.onClick.RemoveListener(
                GoRight
            );
        }
    }
}