using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCDrinkCupController : MonoBehaviour
{
    // =========================================================
    // BOYUTA GÖRE ELDE KONUM AYARI
    // =========================================================

    [Serializable]
    public class HandCupSettings
    {
        public CupSize size;

        [Header("Elde Konum")]
        public Vector3 handPosition;

        [Header("Elde Rotasyon")]
        public Vector3 handRotation;

        [Header("Elde Boyut Çarpanı")]
        public float handScaleMultiplier = 1f;
    }

    [Header("Bardak Tutma")]
    [SerializeField] private Transform drinkCupHolder;

    [Header("Boyuta Göre Elde Konum Ayarları")]
    [SerializeField]
    private List<HandCupSettings> handSettingsPerSize =
        new List<HandCupSettings>();

    private PickupItem currentCup;

    // Kupanın masadaki durumu
    private Transform originalParent;
    private Vector3 originalWorldPosition;
    private Quaternion originalWorldRotation;
    private Vector3 originalLocalScale;

    private bool isCupInHand = false;

    // =========================================================
    // BARDAK ATA
    // =========================================================

    public void SetCup(PickupItem cup)
    {
        currentCup = cup;
        isCupInHand = false;

        Debug.Log(
            $"{gameObject.name}: İçilecek bardak atandı → " +
            $"{cup?.name}"
        );
    }

    // =========================================================
    // ELE AL
    // =========================================================

    public void TakeCup()
    {
        if (currentCup == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: TakeCup çağrıldı ama currentCup yok."
            );

            return;
        }

        if (drinkCupHolder == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: DrinkCupHolder atanmadı!"
            );

            return;
        }

        if (isCupInHand)
        {
            return;
        }

        DrinkRecipe recipe =
            currentCup.GetComponent<DrinkRecipe>();

        if (recipe == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Bardakta DrinkRecipe bulunamadı."
            );

            return;
        }

        HandCupSettings settings =
            handSettingsPerSize.Find(
                s => s.size == recipe.Size
            );

        if (settings == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: {recipe.Size} için Hand Settings yok!"
            );

            return;
        }

        // =====================================================
        // MASA KONUMUNU KAYDET
        // =====================================================

        originalParent =
            currentCup.transform.parent;

        originalWorldPosition =
            currentCup.transform.position;

        originalWorldRotation =
            currentCup.transform.rotation;

        originalLocalScale =
            currentCup.transform.localScale;

        // =====================================================
        // OYUNCU ETKİLEŞİMİNİ GEÇİCİ KİLİTLE
        // =====================================================

        currentCup.SetInteractionLocked(true);

        // =====================================================
        // ELE AL
        // =====================================================

        currentCup.transform.SetParent(
            drinkCupHolder,
            false
        );

        currentCup.transform.localPosition =
            settings.handPosition;

        currentCup.transform.localRotation =
            Quaternion.Euler(
                settings.handRotation
            );

        currentCup.transform.localScale =
            originalLocalScale *
            settings.handScaleMultiplier;

        isCupInHand = true;

        Debug.Log(
            $"{gameObject.name}: " +
            $"{recipe.Size} bardağı eline aldı."
        );
    }

    // =========================================================
    // MASAYA BIRAK
    // =========================================================

    public void PutCupOnTable()
    {
        if (currentCup == null)
            return;

        if (!isCupInHand)
            return;

        RestoreCupToTable();
    }

    // =========================================================
    // BARDAĞI MASAYA GERİ DÖNDÜR
    // =========================================================

    private void RestoreCupToTable()
    {
        if (currentCup == null)
            return;

        currentCup.transform.SetParent(
            originalParent,
            true
        );

        currentCup.transform.position =
            originalWorldPosition;

        currentCup.transform.rotation =
            originalWorldRotation;

        currentCup.transform.localScale =
            originalLocalScale;

        // Tekrar oyuncunun alabilmesine izin ver
        currentCup.SetInteractionLocked(false);

        isCupInHand = false;

        Debug.Log(
            $"{gameObject.name}: Bardak tekrar masaya bırakıldı " +
            "ve oyuncu tarafından alınabilir."
        );
    }

    // =========================================================
    // DIŞARIDAN ZORLA BIRAKTIR
    // =========================================================

    public void ForcePutCupOnTable()
    {
        if (currentCup == null)
            return;

        if (!isCupInHand)
        {
            currentCup.SetInteractionLocked(false);
            return;
        }

        RestoreCupToTable();
    }

    // =========================================================
    // TEMİZLE
    // =========================================================

    public void ClearCup()
    {
        if (currentCup != null)
        {
            currentCup.SetInteractionLocked(false);
        }

        currentCup = null;
        isCupInHand = false;
    }

    // =========================================================
    // GETTER
    // =========================================================

    public PickupItem CurrentCup =>
        currentCup;

    public bool IsCupInHand =>
        isCupInHand;
}