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

    // Sütün toplam ısınma süresi (saniye).
    // 0 = hiç ısıtılmadı.
    private float heatSeconds = 0f;

    // Süt boşalınca eski rengine dönmek için.
    private Material originalMilkMaterial;


    // =========================================================
    // GETTERS
    // =========================================================

    public bool IsFull => isFull;

    public bool IsFilling => isFilling;

    public bool HasMilk =>
        currentProgress > 0.01f;

    public float HeatSeconds =>
        heatSeconds;

    public MilkHeatState HeatState =>
        MilkHeatRules.GetState(heatSeconds);

    // Süt en az bir kez ısıtıldı mı?
    // (CupPourInteraction hâlâ bunu kullanıyor.)
    public bool IsFrothed =>
        heatSeconds > 0.01f;

    public bool IsBurnt =>
        HeatState == MilkHeatState.Burnt;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (milkRenderer != null)
        {
            originalMilkMaterial =
                milkRenderer.sharedMaterial;
        }

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
        heatSeconds = 0f;

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

        // Süt bittiği için ısı da sıfırlanır.
        heatSeconds = 0f;

        // Süt rengi de eski haline dönsün.
        if (milkRenderer != null &&
            originalMilkMaterial != null)
        {
            milkRenderer.material =
                originalMilkMaterial;
        }

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
    // ISI UYGULA (SteamButton çağırır)
    // =========================================================

    public void ApplyHeat(
        float seconds,
        Material heatedMaterial,
        Material burntMaterial)
    {
        heatSeconds =
            Mathf.Clamp(
                seconds,
                0f,
                MilkHeatRules.MaxSeconds
            );

        if (milkRenderer != null)
        {
            if (HeatState == MilkHeatState.Burnt &&
                burntMaterial != null)
            {
                milkRenderer.material =
                    burntMaterial;
            }
            else if (IsFrothed &&
                     heatedMaterial != null)
            {
                milkRenderer.material =
                    heatedMaterial;
            }
        }

        Debug.Log(
            $"Süt ısındı: {heatSeconds:0.0} sn → {HeatState}"
        );
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