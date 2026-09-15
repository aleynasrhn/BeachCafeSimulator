using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCRandomMaterial : MonoBehaviour
{
    // =========================================================
    // KOMBİN (KIYAFET + SAÇ EŞLEŞMESİ)
    // =========================================================

    [Serializable]
    public class OutfitCombo
    {
        [Tooltip("Bu kombinin adı (sadece Inspector'da tanımak için)")]
        public string comboName;

        public Texture2D clothesTexture;

        public Texture2D hairTexture;
    }

    [Header("Kıyafet Materyali")]
    [SerializeField] private Material clothesMaterial;

    [Header("Saç Materyali")]
    [SerializeField] private Material hairMaterial;

    [Header("Kombinler (Kıyafet + Saç Eşleşmeli)")]
    [SerializeField]
    private List<OutfitCombo> outfitCombos =
        new List<OutfitCombo>();


    private void Awake()
    {
        RandomizeTextures();
    }


    public void RandomizeTextures()
    {
        if (outfitCombos == null ||
            outfitCombos.Count == 0)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Hiç kombin tanımlanmamış!"
            );

            return;
        }

        OutfitCombo selectedCombo =
            outfitCombos[
                UnityEngine.Random.Range(0, outfitCombos.Count)
            ];

        // =====================================================
        // KIYAFET
        // =====================================================

        if (clothesMaterial != null &&
            selectedCombo.clothesTexture != null)
        {
            clothesMaterial.SetTexture(
                "_BaseMap",
                selectedCombo.clothesTexture
            );

            Debug.Log(
                $"{gameObject.name} kıyafet texture: " +
                selectedCombo.clothesTexture.name
            );
        }

        // =====================================================
        // SAÇ
        // =====================================================

        if (hairMaterial != null &&
            selectedCombo.hairTexture != null)
        {
            hairMaterial.SetTexture(
                "_BaseMap",
                selectedCombo.hairTexture
            );

            Debug.Log(
                $"{gameObject.name} saç texture: " +
                selectedCombo.hairTexture.name
            );
        }

        Debug.Log(
            $"{gameObject.name} kombin seçildi: " +
            (string.IsNullOrEmpty(selectedCombo.comboName)
                ? "(isimsiz)"
                : selectedCombo.comboName)
        );
    }
}