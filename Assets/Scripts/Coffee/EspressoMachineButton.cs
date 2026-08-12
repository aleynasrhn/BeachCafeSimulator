using UnityEngine;
using System.Collections;

/// <summary>
/// Espresso makinesindeki fiziksel düğmeye/kola eklenir (yeni bir GameObject oluşturup
/// koyacaksın - Unity kurulumu talimatlarına bak). Tek basışlı normal bir IInteractable.
///
/// Sadece PortafilterDock'ta dolu+tamperlenmiş bir portafilter TAKILIYKEN çalışır.
/// Basılınca: dökülme görselini (varsa) birkaç saniye açık tutar, süre bitince
/// CupDock'ta bir cup varsa onu espressoyla doldurur.
/// </summary>
public class EspressoMachineButton : MonoBehaviour, IInteractable
{
    [Header("Referanslar")]
    [Tooltip("Bu makinenin PortafilterDock'u - dolu+tamperlenmiş portafilter var mı diye kontrol için")]
    [SerializeField] private MachineDockPoint portafilterDock;
    [Tooltip("Bu makinenin CupDock'u - brew bitince cup doldurmak için (boş bırakabilirsin, cup şart değilse)")]
    [SerializeField] private MachineDockPoint cupDock;
    [Tooltip("Dökülme sırasında açılacak görsel (basit bir mesh, particle system, ya da stretched cylinder). Opsiyonel, boş bırakabilirsin.")]
    [SerializeField] private GameObject pouringVisual;

    [Header("Ayarlar")]
    [SerializeField] private float brewDuration = 4f;

    private bool isBrewing = false;

    public string GetInteractPrompt()
    {
        if (isBrewing) return "Demleniyor...";
        if (portafilterDock == null || !portafilterDock.IsOccupied) return "Önce portafilteri tak";
        return "E - Espresso Başlat";
    }

    public void Interact(PlayerInteraction player)
    {
        if (isBrewing) return; // zaten çalışıyor, tekrar tetiklenemez
        if (portafilterDock == null || !portafilterDock.IsOccupied) return; // portafilter takılı değil

        StartCoroutine(BrewRoutine());
    }

    private IEnumerator BrewRoutine()
    {
        isBrewing = true;

        if (pouringVisual != null)
            pouringVisual.SetActive(true);

        yield return new WaitForSeconds(brewDuration);

        if (pouringVisual != null)
            pouringVisual.SetActive(false);

        // Cup takılıysa doldur
        if (cupDock != null && cupDock.IsOccupied && cupDock.DockedItem != null)
        {
            cupDock.DockedItem.FillWithEspresso();
        }

        isBrewing = false;
    }
}