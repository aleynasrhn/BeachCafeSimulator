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
    [SerializeField] private List<HandCupSettings> handSettingsPerSize = new List<HandCupSettings>();


    private PickupItem currentCup;

    // Kupanın masadaki (ele alınmadan önceki) orijinal durumu.
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
    }


    // =========================================================
    // BARDAĞI ELE AL
    // =========================================================
    //
    // BU METODU "Drinking" ANİMASYONUNDAKİ TakeCup EVENT'İ ÇAĞIRIYOR.
    // Her tekrar (2. kez içme vb.) için de otomatik olarak yeniden
    // çağrılır çünkü Animation Event klibe bağlı, script'e değil.
    //
    // =========================================================

    public void TakeCup()
    {
        if (currentCup == null)
            return;

        if (drinkCupHolder == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: DrinkCupHolder atanmadı!"
            );

            return;
        }

        if (isCupInHand)
        {
            // Zaten elde, tekrar alma.
            return;
        }


        // =====================================================
        // BOYUTU ÖĞREN
        // =====================================================

        DrinkRecipe recipe =
            currentCup.GetComponent<DrinkRecipe>();

        if (recipe == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Alınan bardakta DrinkRecipe " +
                "bulunamadı, elde konumlandırma yapılamıyor."
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
                $"{gameObject.name}: {recipe.Size} boyutu için " +
                "Hand Settings tanımlanmamış!"
            );

            return;
        }


        // =====================================================
        // ELE ALMADAN ÖNCEKİ (MASADAKİ) KONUMU KAYDET
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
        // ELE YERLEŞTİR
        // =====================================================

        currentCup.transform.SetParent(
            drinkCupHolder,
            false
        );

        currentCup.transform.localPosition =
            settings.handPosition;

        currentCup.transform.localRotation =
            Quaternion.Euler(settings.handRotation);

        currentCup.transform.localScale =
            originalLocalScale *
            settings.handScaleMultiplier;

        isCupInHand = true;

        Debug.Log(
            $"{gameObject.name} {recipe.Size} bardağı eline aldı."
        );
    }


    // =========================================================
    // BARDAĞI MASAYA BIRAK
    // =========================================================
    //
    // BU METODU "Drinking" ANİMASYONUNDAKİ BIRAKMA EVENT'İ ÇAĞIRACAK.
    // Animator penceresinde ilgili event'in Function alanına
    // bu metodu (PutCupOnTable) seçmen gerekiyor.
    //
    // =========================================================

    public void PutCupOnTable()
    {
        if (currentCup == null)
            return;

        if (!isCupInHand)
        {
            // Zaten elde değil, bırakılacak bir şey yok.
            return;
        }


        currentCup.transform.SetParent(
            originalParent
        );

        currentCup.transform.position =
            originalWorldPosition;

        currentCup.transform.rotation =
            originalWorldRotation;

        currentCup.transform.localScale =
            originalLocalScale;

        isCupInHand = false;

        Debug.Log(
            $"{gameObject.name} bardağı masaya bıraktı."
        );
    }
}