using UnityEngine;
using System.Collections;

public class MilkFiller : MonoBehaviour
{
    [Header("Süt Dolumu")]
    public Transform milkFillTransform;

    [SerializeField]
    private float fillDuration = 2f;

    [SerializeField]
    private float maxFillScaleY = 1f;

    [SerializeField]
    private float minFillY = 0.01f;


    [Header("Buharlandırma")]
    [Tooltip("MilkFill objesinin kendi Renderer'ı")]
    public Renderer milkRenderer;


    private Vector3 basePosition;

    private bool isFilling = false;
    private bool isFull = false;

    // 0 = boş
    // 1 = tam dolu
    private float currentProgress = 0f;

    private bool isFrothed = false;


    // =========================================================
    // GETTERS
    // =========================================================

    public bool IsFull => isFull;

    public bool IsFilling => isFilling;

    public bool HasMilk =>
        currentProgress > 0.01f;

    public bool IsFrothed =>
        isFrothed;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (milkFillTransform == null)
        {
            Debug.LogError(
                $"{gameObject.name}: MilkFill Transform atanmadı!"
            );

            return;
        }

        basePosition =
            milkFillTransform.localPosition;

        currentProgress = 0f;
        isFull = false;
        isFilling = false;
        isFrothed = false;

        milkFillTransform.localPosition =
            basePosition;

        milkFillTransform.localScale =
            new Vector3(
                milkFillTransform.localScale.x,
                minFillY,
                milkFillTransform.localScale.z
            );
    }


    // =========================================================
    // DOLUM İLERLEMESİ
    // =========================================================

    public void SetFillProgress(
        float progress01)
    {
        if (milkFillTransform == null)
            return;

        if (isFull)
            return;

        isFilling = true;

        currentProgress =
            Mathf.Clamp01(progress01);

        UpdateMilkVisual();
    }


    // =========================================================
    // DOLUM TAMAMLANDI
    // =========================================================

    public void CompleteFill()
    {
        currentProgress = 1f;

        isFilling = false;
        isFull = true;

        UpdateMilkVisual();

        Debug.Log(
            "Pitcher tamamen sütle doldu."
        );
    }


    // =========================================================
    // TÜM SÜTÜ TÜKET
    // =========================================================

    public bool ConsumeAllMilk()
    {
        if (!HasMilk)
            return false;

        currentProgress = 0f;

        isFull = false;
        isFilling = false;

        // Süt tamamen bittiği için
        // köpüklü durum da sıfırlanır.
        isFrothed = false;

        UpdateMilkVisual();

        Debug.Log(
            "Pitcher'daki süt tamamen tüketildi."
        );

        return true;
    }


    // =========================================================
    // GÖRSEL GÜNCELLE
    // =========================================================

    private void UpdateMilkVisual()
    {
        if (milkFillTransform == null)
            return;

        milkFillTransform.localPosition =
            basePosition;

        float newScaleY =
            Mathf.Lerp(
                minFillY,
                maxFillScaleY,
                currentProgress
            );

        milkFillTransform.localScale =
            new Vector3(
                milkFillTransform.localScale.x,
                newScaleY,
                milkFillTransform.localScale.z
            );
    }


    // =========================================================
    // KÖPÜRT
    // =========================================================

    public void SetFrothedMaterial(
        Material frothedMaterial)
    {
        if (milkRenderer != null &&
            frothedMaterial != null)
        {
            milkRenderer.material =
                frothedMaterial;
        }

        isFrothed = true;
    }


    // =========================================================
    // PITCHER DOLDURMAYI BAŞLAT
    // =========================================================

    public void StartPouring()
    {
        if (isFull)
            return;

        if (isFilling)
            return;

        StartCoroutine(
            FillPitcher()
        );
    }


    // =========================================================
    // PITCHER DOLDUR
    // =========================================================

    private IEnumerator FillPitcher()
    {
        if (milkFillTransform == null)
            yield break;

        isFilling = true;

        float elapsed = 0f;

        float startScaleY =
            milkFillTransform.localScale.y;

        milkFillTransform.localPosition =
            basePosition;

        while (
            elapsed < fillDuration
        )
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / fillDuration
                );

            float newScaleY =
                Mathf.Lerp(
                    startScaleY,
                    maxFillScaleY,
                    t
                );

            milkFillTransform.localScale =
                new Vector3(
                    milkFillTransform.localScale.x,
                    newScaleY,
                    milkFillTransform.localScale.z
                );

            yield return null;
        }

        milkFillTransform.localScale =
            new Vector3(
                milkFillTransform.localScale.x,
                maxFillScaleY,
                milkFillTransform.localScale.z
            );

        currentProgress = 1f;

        isFilling = false;
        isFull = true;

        Debug.Log(
            "Pitcher is full!"
        );
    }


    // =========================================================
    // TEST
    // =========================================================

    private void Update()
    {
        // GEÇİCİ TEST
        // Daha sonra kaldırabiliriz.
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartPouring();
        }
    }
}