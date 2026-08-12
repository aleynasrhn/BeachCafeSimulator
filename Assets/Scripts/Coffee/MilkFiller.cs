using UnityEngine;
using System.Collections;

public class MilkFiller : MonoBehaviour
{
    public Transform milkFillTransform;   // MilkFill cylinder'ının Transform'u
    public float fillDuration = 2f;        // Kaç saniyede dolsun
    public float maxFillScaleY = 1f;       // Tam dolu haldeki Y scale değeri
    public float minFillY = 0.01f;         // Boşken Y scale değeri

    private Vector3 basePosition;          // SABİT pozisyon - hiç değişmeyecek
    private bool isFilling = false;
    private bool isFull = false;
    private float currentProgress = 0f;

    [Header("Buharlandırma (opsiyonel)")]
    [Tooltip("MilkFill objesinin kendi Renderer'ı - buharlandırınca materyalini değiştirmek için")]
    public Renderer milkRenderer;

    public bool IsFull => isFull;
    public bool IsFilling => isFilling;
    public bool HasMilk => currentProgress > 0.01f; // biraz bile süt varsa true
    public bool IsFrothed => isFrothed;

    private bool isFrothed = false;

    /// <summary>
    /// MilkPourInteraction tarafından E basılı tutulduğu sürece HER FRAME çağrılır.
    /// progress01: 0 (boş) -> 1 (tam dolu). Coroutine YOK, direkt anlık scale ayarı.
    /// </summary>
    public void SetFillProgress(float progress01)
    {
        if (milkFillTransform == null || isFull) return;

        isFilling = true;
        currentProgress = Mathf.Clamp01(progress01);
        milkFillTransform.localPosition = basePosition; // pozisyon hep sabit kalsın

        float newScaleY = Mathf.Lerp(minFillY, maxFillScaleY, currentProgress);
        milkFillTransform.localScale = new Vector3(
            milkFillTransform.localScale.x,
            newScaleY,
            milkFillTransform.localScale.z
        );
    }

    /// <summary>Basılı tutma tamamlanınca çağrılır - dolumu kesinleştirir.</summary>
    public void CompleteFill()
    {
        isFilling = false;
        isFull = true;
    }

    /// <summary>SteamButton tarafından çağrılır - sütün görünümünü "köpüklü/buharlanmış" materyale çevirir.</summary>
    public void SetFrothedMaterial(Material frothedMaterial)
    {
        if (milkRenderer != null && frothedMaterial != null)
            milkRenderer.material = frothedMaterial;

        isFrothed = true;
    }

    void Start()
    {
        basePosition = milkFillTransform.localPosition;
        milkFillTransform.localScale = new Vector3(
            milkFillTransform.localScale.x,
            minFillY,
            milkFillTransform.localScale.z
        );
    }

    public void StartPouring()
    {
        if (!isFull && !isFilling)
        {
            StartCoroutine(FillPitcher());
        }
    }

    IEnumerator FillPitcher()
    {
        isFilling = true;
        float elapsed = 0f;
        float startScaleY = milkFillTransform.localScale.y;

        // Pozisyonu BİR KERE sabitliyoruz, döngü boyunca bir daha dokunmuyoruz
        milkFillTransform.localPosition = basePosition;

        while (elapsed < fillDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fillDuration;

            float newScaleY = Mathf.Lerp(startScaleY, maxFillScaleY, t);

            milkFillTransform.localScale = new Vector3(
                milkFillTransform.localScale.x,
                newScaleY,
                milkFillTransform.localScale.z
            );

            // Pozisyona ARTIK DOKUNMUYORUZ - sabit kalıyor

            yield return null;
        }

        isFilling = false;
        isFull = true;
        Debug.Log("Pitcher is full!");
    }

    void Update()
    {
        // GEÇİCİ TEST KODU - test bitince sil
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartPouring();
        }
    }
}