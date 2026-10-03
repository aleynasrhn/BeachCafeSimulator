using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ekranın ortasındaki crosshair'i, oyuncu geçerli bir hedefe (item, tezgah, dock, grinder vs.)
/// bakarken büyütür/renk değiştirir - nişan almayı kolaylaştırır.
/// Canvas'taki Crosshair objesine ekle, PlayerInteraction referansını sürükle.
/// </summary>
public class CrosshairUI : MonoBehaviour
{
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private Image crosshairImage;

    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float targetScale = 1.6f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color targetColor = new Color(1f, 0.85f, 0.3f); // sarımsı - "buraya bakıyorsun" hissi

    private void Update()
    {
        if (playerInteraction == null || crosshairImage == null) return;

        bool isLookingAtSomething = playerInteraction.IsLookingAtInteractable;

        float scale = isLookingAtSomething ? targetScale : normalScale;
        crosshairImage.transform.localScale = Vector3.one * scale;
        crosshairImage.color = isLookingAtSomething ? targetColor : normalColor;
    }
}