using UnityEngine;

public class CupLiquidVisual : MonoBehaviour
{
    [Header("Sıvı Renderer")]
    [SerializeField] private Renderer liquidRenderer;


    // =========================================================
    // BOŞ
    // =========================================================

    [Header("BOŞ")]

    [SerializeField]
    private Vector3 emptyPosition =
        new Vector3(
            -1.392398f,
            -0.0002f,
            -0.014f
        );

    [SerializeField]
    private Vector3 emptyScale =
        new Vector3(
            -0.02014265f,
            0.00075f,
            -0.02014265f
        );


    // =========================================================
    // SADECE ESPRESSO
    // =========================================================

    [Header("ESPRESSO")]

    [SerializeField]
    private Vector3 espressoPosition =
        new Vector3(
            -1.392398f,
            -0.0002f,
            0.00537f
        );

    [SerializeField]
    private Vector3 espressoScale =
        new Vector3(
            -0.025f,
            0.00075f,
            -0.025f
        );


    // =========================================================
    // TAM DOLU
    // ESPRESSO + SÜT / SU / KÖPÜKLÜ SÜT
    // =========================================================

    [Header("TAM DOLU")]

    [SerializeField]
    private Vector3 fullPosition =
        new Vector3(
            -1.392398f,
            -0.0002f,
            0.00984f
        );

    [SerializeField]
    private Vector3 fullScale =
        new Vector3(
            -0.026f,
            0.00075f,
            -0.026f
        );


    // =========================================================
    // İÇECEK MATERIALARI
    // =========================================================

    [Header("İçecek Materialları")]

    [Tooltip("Sadece espresso")]
    [SerializeField]
    private Material espressoMaterial;

    [Tooltip("Espresso + normal süt = Latte")]
    [SerializeField]
    private Material espressoMilkMaterial;

    [Tooltip("Espresso + su = Americano")]
    [SerializeField]
    private Material espressoWaterMaterial;

    [Tooltip("Espresso + köpüklü süt = Cappuccino")]
    [SerializeField]
    private Material espressoFrothedMilkMaterial;


    // =========================================================
    // BAŞLANGIÇ
    // =========================================================

    private void Awake()
    {
        if (liquidRenderer == null)
        {
            liquidRenderer =
                GetComponent<Renderer>();
        }

        transform.localPosition =
            emptyPosition;

        transform.localScale =
            emptyScale;

        if (liquidRenderer != null)
        {
            liquidRenderer.enabled = false;
        }
    }


    // =========================================================
    // ESPRESSO
    // =========================================================

    public void SetEspressoProgress(
        float progress01)
    {
        progress01 =
            Mathf.Clamp01(
                progress01
            );

        ShowRenderer();

        if (espressoMaterial != null)
        {
            liquidRenderer.material =
                espressoMaterial;
        }

        transform.localPosition =
            Vector3.Lerp(
                emptyPosition,
                espressoPosition,
                progress01
            );

        transform.localScale =
            Vector3.Lerp(
                emptyScale,
                espressoScale,
                progress01
            );
    }


    // =========================================================
    // NORMAL SÜT
    // ESPRESSO + SÜT = LATTE
    // =========================================================

    public void SetMilkProgress(
        float progress01)
    {
        progress01 =
            Mathf.Clamp01(
                progress01
            );

        ShowRenderer();

        if (espressoMilkMaterial != null)
        {
            liquidRenderer.material =
                espressoMilkMaterial;
        }

        transform.localPosition =
            Vector3.Lerp(
                espressoPosition,
                fullPosition,
                progress01
            );

        transform.localScale =
            Vector3.Lerp(
                espressoScale,
                fullScale,
                progress01
            );
    }


    // =========================================================
    // SU
    // ESPRESSO + SU = AMERICANO
    // =========================================================

    public void SetWaterProgress(
        float progress01)
    {
        progress01 =
            Mathf.Clamp01(
                progress01
            );

        ShowRenderer();

        if (espressoWaterMaterial != null)
        {
            liquidRenderer.material =
                espressoWaterMaterial;
        }

        transform.localPosition =
            Vector3.Lerp(
                espressoPosition,
                fullPosition,
                progress01
            );

        transform.localScale =
            Vector3.Lerp(
                espressoScale,
                fullScale,
                progress01
            );
    }


    // =========================================================
    // KÖPÜKLÜ SÜT
    // ESPRESSO + KÖPÜKLÜ SÜT = CAPPUCCINO
    // =========================================================

    public void SetFrothedMilkProgress(
        float progress01)
    {
        progress01 =
            Mathf.Clamp01(
                progress01
            );

        ShowRenderer();

        if (espressoFrothedMilkMaterial != null)
        {
            liquidRenderer.material =
                espressoFrothedMilkMaterial;
        }

        transform.localPosition =
            Vector3.Lerp(
                espressoPosition,
                fullPosition,
                progress01
            );

        transform.localScale =
            Vector3.Lerp(
                espressoScale,
                fullScale,
                progress01
            );
    }


    // =========================================================
    // RENDERER
    // =========================================================

    private void ShowRenderer()
    {
        if (liquidRenderer != null)
        {
            liquidRenderer.enabled = true;
        }
    }


    // =========================================================
    // BOŞALT
    // =========================================================

    public void Hide()
    {
        transform.localPosition =
            emptyPosition;

        transform.localScale =
            emptyScale;

        if (liquidRenderer != null)
        {
            liquidRenderer.enabled = false;
        }
    }


    // =========================================================
    // TESTLER
    // =========================================================

    [ContextMenu("Test Espresso")]
    public void TestEspresso()
    {
        SetEspressoProgress(1f);
    }


    [ContextMenu("Test Latte")]
    public void TestLatte()
    {
        SetMilkProgress(1f);
    }


    [ContextMenu("Test Americano")]
    public void TestAmericano()
    {
        SetWaterProgress(1f);
    }


    [ContextMenu("Test Cappuccino")]
    public void TestCappuccino()
    {
        SetFrothedMilkProgress(1f);
    }
}