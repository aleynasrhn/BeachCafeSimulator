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
        new Vector3(-1.392398f, -0.0002f, -0.014f);

    [SerializeField]
    private Vector3 emptyScale =
        new Vector3(-0.02014265f, 0.00075f, -0.02014265f);


    // =========================================================
    // ESPRESSO
    // =========================================================

    [Header("ESPRESSO")]

    [SerializeField]
    private Vector3 espressoPosition =
        new Vector3(-1.392398f, -0.0002f, 0.00537f);

    [SerializeField]
    private Vector3 espressoScale =
        new Vector3(-0.025f, 0.00075f, -0.025f);


    // =========================================================
    // TAM DOLU
    // ESPRESSO + SÜT / SU
    // =========================================================

    [Header("TAM DOLU")]

    [SerializeField]
    private Vector3 fullPosition =
        new Vector3(-1.392398f, -0.0002f, 0.00984f);

    [SerializeField]
    private Vector3 fullScale =
        new Vector3(-0.026f, 0.00075f, -0.026f);


    // =========================================================
    // MATERIAL
    // =========================================================

    [Header("Materiallar")]

    [SerializeField] private Material espressoMaterial;
    [SerializeField] private Material milkMaterial;
    [SerializeField] private Material frothedMilkMaterial;


    // =========================================================
    // BAŞLANGIÇ
    // =========================================================

    private void Awake()
    {
        if (liquidRenderer == null)
        {
            liquidRenderer = GetComponent<Renderer>();
        }

        transform.localPosition = emptyPosition;
        transform.localScale = emptyScale;

        if (liquidRenderer != null)
        {
            liquidRenderer.enabled = false;
        }
    }


    // =========================================================
    // ESPRESSO DOLUMU
    // =========================================================

    public void SetEspressoProgress(float progress01)
    {
        progress01 = Mathf.Clamp01(progress01);

        if (liquidRenderer != null)
        {
            liquidRenderer.enabled = true;

            if (espressoMaterial != null)
            {
                liquidRenderer.material = espressoMaterial;
            }
        }

        transform.localPosition = Vector3.Lerp(
            emptyPosition,
            espressoPosition,
            progress01
        );

        transform.localScale = Vector3.Lerp(
            emptyScale,
            espressoScale,
            progress01
        );
    }


    // =========================================================
    // SÜT DOLUMU
    // =========================================================

    public void SetMilkProgress(float progress01)
    {
        progress01 = Mathf.Clamp01(progress01);

        if (liquidRenderer != null)
        {
            liquidRenderer.enabled = true;

            if (milkMaterial != null)
            {
                liquidRenderer.material = milkMaterial;
            }
        }

        transform.localPosition = Vector3.Lerp(
            espressoPosition,
            fullPosition,
            progress01
        );

        transform.localScale = Vector3.Lerp(
            espressoScale,
            fullScale,
            progress01
        );
    }


    // =========================================================
    // KÖPÜKLÜ SÜT
    // =========================================================

    public void SetFrothedMilkProgress(float progress01)
    {
        progress01 = Mathf.Clamp01(progress01);

        if (liquidRenderer != null)
        {
            liquidRenderer.enabled = true;

            if (frothedMilkMaterial != null)
            {
                liquidRenderer.material = frothedMilkMaterial;
            }
        }

        transform.localPosition = Vector3.Lerp(
            espressoPosition,
            fullPosition,
            progress01
        );

        transform.localScale = Vector3.Lerp(
            espressoScale,
            fullScale,
            progress01
        );
    }


    // =========================================================
    // BOŞALT
    // =========================================================

    public void Hide()
    {
        transform.localPosition = emptyPosition;
        transform.localScale = emptyScale;

        if (liquidRenderer != null)
        {
            liquidRenderer.enabled = false;
        }
    }


    // =========================================================
    // TEST
    // =========================================================

    [ContextMenu("Test Espresso")]
    public void TestEspresso()
    {
        SetEspressoProgress(1f);
    }

    [ContextMenu("Test Full")]
    public void TestFull()
    {
        transform.localPosition = fullPosition;
        transform.localScale = fullScale;

        if (liquidRenderer != null)
        {
            liquidRenderer.enabled = true;
        }
    }
}