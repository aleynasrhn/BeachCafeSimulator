using UnityEngine;
using System.Collections;

/// <summary>
/// Espresso makinesinin başlatma düğmesini kontrol eder.
///
/// Şartlar:
/// - Portafilter takılı olmalı.
/// - Portafilter kahveli olmalı.
/// - Portafilter tamp edilmiş olmalı.
/// - Portafilter kullanılmış kahve içermemeli.
/// - Espresso shot bardağı takılı olmalı.
/// - Single veya Double seçilmiş olmalı.
///
/// Brew sonunda espresso shot bardağına
/// seçilen miktarda espresso yüklenir.
///
/// Single = 1 shot
/// Double = 2 shot
/// </summary>
public class EspressoMachineButton : MonoBehaviour, IInteractable
{
    [Header("Referanslar")]

    [Tooltip("Makinedeki portafilter dock'u.")]
    [SerializeField] private MachineDockPoint portafilterDock;

    [Tooltip("Makinedeki espresso shot bardağı dock'u.")]
    [SerializeField] private MachineDockPoint cupDock;

    [Tooltip("Espresso akışı sırasında gösterilecek görsel.")]
    [SerializeField] private GameObject pouringVisual;

    [Tooltip("Makinedeki fiziksel Single butonu.")]
    [SerializeField] private EspressoShotButton singleButton;

    [Tooltip("Makinedeki fiziksel Double butonu.")]
    [SerializeField] private EspressoShotButton doubleButton;


    [Header("Ayarlar")]

    [Tooltip("Espresso hazırlama süresi.")]
    [SerializeField] private float brewDuration = 4f;


    // =========================================================
    // DURUM
    // =========================================================

    private bool isBrewing = false;


    // =========================================================
    // PROMPT
    // =========================================================

    public string GetInteractPrompt()
    {
        if (isBrewing)
            return "Demleniyor...";


        // -----------------------------------------------------
        // PORTAFILTER
        // -----------------------------------------------------

        if (portafilterDock == null ||
            !portafilterDock.IsOccupied ||
            portafilterDock.DockedItem == null)
        {
            return "Önce portafilteri tak";
        }


        PickupItem portafilter =
            portafilterDock.DockedItem;


        // -----------------------------------------------------
        // USED COFFEE
        // -----------------------------------------------------

        if (portafilter.HasUsedCoffee)
        {
            return "Önce kullanılmış kahveyi temizle";
        }


        // -----------------------------------------------------
        // ESPRESSO SHOT BARDAĞI
        // -----------------------------------------------------

        if (cupDock == null ||
            !cupDock.IsOccupied ||
            cupDock.DockedItem == null)
        {
            return "Önce espresso shot bardağını koy";
        }


        // -----------------------------------------------------
        // SHOT
        // -----------------------------------------------------

        EspressoShotButton selectedShot =
            EspressoShotButton.GetSelectedButton();


        if (selectedShot == null)
        {
            return "Single veya Double seç";
        }


        Debug.Log(
            "MAKİNE SEÇİLEN SHOT: " +
            selectedShot.Shot
        );


        return "E - Espresso Başlat";
    }


    // =========================================================
    // INTERACT
    // =========================================================

    public void Interact(PlayerInteraction player)
    {
        if (isBrewing)
            return;


        // -----------------------------------------------------
        // PORTAFILTER
        // -----------------------------------------------------

        if (portafilterDock == null ||
            !portafilterDock.IsOccupied ||
            portafilterDock.DockedItem == null)
        {
            return;
        }


        PickupItem portafilter =
            portafilterDock.DockedItem;


        // -----------------------------------------------------
        // USED COFFEE
        // -----------------------------------------------------

        if (portafilter.HasUsedCoffee)
        {
            return;
        }


        // -----------------------------------------------------
        // SHOT BARDAĞI
        // -----------------------------------------------------

        if (cupDock == null ||
            !cupDock.IsOccupied ||
            cupDock.DockedItem == null)
        {
            return;
        }


        // -----------------------------------------------------
        // SHOT
        // -----------------------------------------------------

        EspressoShotButton selectedShot =
            EspressoShotButton.GetSelectedButton();


        if (selectedShot == null)
        {
            return;
        }


        // -----------------------------------------------------
        // BREW BAŞLAT
        // -----------------------------------------------------

        StartCoroutine(
            BrewRoutine(
                selectedShot.Shot
            )
        );
    }


    // =========================================================
    // BREW
    // =========================================================

    private IEnumerator BrewRoutine(
        EspressoShotButton.ShotType shotType)
    {
        isBrewing = true;


        // -----------------------------------------------------
        // DOCKLARI KİLİTLE
        // -----------------------------------------------------

        if (portafilterDock != null)
        {
            portafilterDock.SetLocked(true);
        }


        if (cupDock != null)
        {
            cupDock.SetLocked(true);
        }


        // -----------------------------------------------------
        // SHOT BUTONLARINI KİLİTLE
        // -----------------------------------------------------

        if (singleButton != null)
        {
            singleButton.SetLocked(true);
        }


        if (doubleButton != null)
        {
            doubleButton.SetLocked(true);
        }


        // -----------------------------------------------------
        // ESPRESSO AKIŞI
        // -----------------------------------------------------

        if (pouringVisual != null)
        {
            pouringVisual.SetActive(true);
        }


        // -----------------------------------------------------
        // SHOT SAYISI
        // -----------------------------------------------------

        int shotCount =
            shotType ==
            EspressoShotButton.ShotType.Double
                ? 2
                : 1;


        Debug.Log(
            $"BREW SHOT: {shotType} | " +
            $"SHOT COUNT: {shotCount}"
        );


        // -----------------------------------------------------
        // BREW SÜRESİ
        // -----------------------------------------------------

        yield return new WaitForSeconds(
            brewDuration
        );


        // -----------------------------------------------------
        // AKIŞI KAPAT
        // -----------------------------------------------------

        if (pouringVisual != null)
        {
            pouringVisual.SetActive(false);
        }


        // -----------------------------------------------------
        // ESPRESSO SHOT BARDAĞINA ESPRESSO YÜKLE
        // -----------------------------------------------------

        if (cupDock != null &&
            cupDock.IsOccupied &&
            cupDock.DockedItem != null)
        {
            PickupItem espressoCup =
                cupDock.DockedItem;


            PourSource espressoSource =
                espressoCup.GetComponent<PourSource>();


            if (espressoSource != null)
            {
                espressoSource.SetEspressoShots(
                    shotCount
                );


                espressoCup.FillWithEspresso();


                Debug.Log(
                    $"ESPRESSO KAYNAĞI HAZIR: " +
                    $"{shotCount} shot"
                );
            }
            else
            {
                Debug.LogError(
                    "ESPRESSO SHOT BARDAĞINDA " +
                    "PourSource bulunamadı!"
                );
            }
        }


        // -----------------------------------------------------
        // PORTAFILTER = USED COFFEE
        // -----------------------------------------------------

        if (portafilterDock != null &&
            portafilterDock.IsOccupied &&
            portafilterDock.DockedItem != null)
        {
            PickupItem portafilter =
                portafilterDock.DockedItem;


            portafilter.MarkCoffeeAsUsed();
        }


        // -----------------------------------------------------
        // DOCKLARI AÇ
        // -----------------------------------------------------

        if (portafilterDock != null)
        {
            portafilterDock.SetLocked(false);
        }


        if (cupDock != null)
        {
            cupDock.SetLocked(false);
        }


        // -----------------------------------------------------
        // SHOT BUTONLARINI AÇ
        // -----------------------------------------------------

        if (singleButton != null)
        {
            singleButton.SetLocked(false);
        }


        if (doubleButton != null)
        {
            doubleButton.SetLocked(false);
        }


        // -----------------------------------------------------
        // SHOT SEÇİMİNİ SIFIRLA
        // -----------------------------------------------------

        EspressoShotButton selectedButton =
            EspressoShotButton.GetSelectedButton();


        if (selectedButton != null)
        {
            selectedButton.SetSelected(false);
        }


        // -----------------------------------------------------
        // BREW BİTTİ
        // -----------------------------------------------------

        isBrewing = false;
    }
}