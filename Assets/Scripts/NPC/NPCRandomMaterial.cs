using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCRandomMaterial : MonoBehaviour
{
    // =========================================================
    // KOMBİN (KIYAFET + BODY + SAÇ EŞLEŞMESİ)
    // =========================================================

    [Serializable]
    public class OutfitCombo
    {
        [Tooltip("Bu kombinin adı. HER KOMBİN İÇİN FARKLI BİR İSİM GİR.")]
        public string comboName;

        // Kıyafet texture
        public Texture2D clothesTexture;

        // Body texture
        // Normal NPC'lerde boş bırakılabilir.
        public Texture2D bodyTexture;

        // Saç / sakal / kirpik texture
        public Texture2D hairTexture;
    }


    // =========================================================
    // KIYAFET / VÜCUT RENDERER'LARI
    // =========================================================

    [Header("Kıyafet Renderer'ları (Materyal Değil!)")]
    [Tooltip("Aynı clothes texture'ını kullanacak tüm mesh parçalarını ekle: Shirt, Pants, Shoes vb.")]
    [SerializeField]
    private List<Renderer> clothesRenderers =
        new List<Renderer>();


    // =========================================================
    // BODY RENDERER'LARI
    // =========================================================

    [Header("Body Renderer'ları (Materyal Değil!)")]
    [Tooltip("Body texture'ını kullanacak mesh parçalarını ekle. Normal NPC'lerde boş bırakılabilir.")]
    [SerializeField]
    private List<Renderer> bodyRenderers =
        new List<Renderer>();


    // =========================================================
    // SAÇ / SAKAL / KİRPİK RENDERER'LARI
    // =========================================================

    [Header("Saç / Sakal / Kirpik Renderer'ları (Materyal Değil!)")]
    [Tooltip("Aynı hair texture'ını kullanacak tüm mesh parçalarını ekle: Hair, Beard, Eyelashes vb.")]
    [SerializeField]
    private List<Renderer> hairRenderers =
        new List<Renderer>();


    // =========================================================
    // KOMBİNLER
    // =========================================================

    [Header("Kombinler (Kıyafet + Body + Saç Eşleşmeli)")]
    [SerializeField]
    private List<OutfitCombo> outfitCombos =
        new List<OutfitCombo>();


    // =========================================================
    // COMBO COOLDOWN
    // =========================================================

    [Header("Aynı Kombinin Tekrar Gelmeme Ayarı")]
    [Tooltip("Bu karakter tekrar spawn olduğunda aynı kombin gelmeden önce en az bu kadar farklı kombin kullanılmış olmalı.")]
    [SerializeField]
    private int comboCooldownCount = 2;


    // =========================================================
    // KARAKTER BAŞINA KOMBİN GEÇMİŞİ
    // =========================================================

    private static readonly Dictionary<string, List<string>>
        comboHistoryByCharacter =
        new Dictionary<string, List<string>>();


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        RandomizeTextures();
    }


    // =========================================================
    // RANDOMIZE
    // =========================================================

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
            SelectRandomCombo();


        // =====================================================
        // KIYAFET
        // =====================================================

        if (selectedCombo.clothesTexture != null &&
            clothesRenderers != null)
        {
            foreach (Renderer rend in clothesRenderers)
            {
                if (rend == null)
                    continue;


                Material clothesInstance =
                    rend.material;


                clothesInstance.SetTexture(
                    "_BaseMap",
                    selectedCombo.clothesTexture
                );
            }


            Debug.Log(
                $"{gameObject.name} kıyafet texture " +
                $"({clothesRenderers.Count} parça): " +
                selectedCombo.clothesTexture.name
            );
        }


        // =====================================================
        // BODY
        // =====================================================

        if (selectedCombo.bodyTexture != null &&
            bodyRenderers != null)
        {
            foreach (Renderer rend in bodyRenderers)
            {
                if (rend == null)
                    continue;


                Material bodyInstance =
                    rend.material;


                bodyInstance.SetTexture(
                    "_BaseMap",
                    selectedCombo.bodyTexture
                );
            }


            Debug.Log(
                $"{gameObject.name} body texture " +
                $"({bodyRenderers.Count} parça): " +
                selectedCombo.bodyTexture.name
            );
        }


        // =====================================================
        // SAÇ / SAKAL / KİRPİK
        // =====================================================

        if (selectedCombo.hairTexture != null &&
            hairRenderers != null)
        {
            foreach (Renderer rend in hairRenderers)
            {
                if (rend == null)
                    continue;


                Material hairInstance =
                    rend.material;


                hairInstance.SetTexture(
                    "_BaseMap",
                    selectedCombo.hairTexture
                );
            }


            Debug.Log(
                $"{gameObject.name} saç/sakal/kirpik texture " +
                $"({hairRenderers.Count} parça): " +
                selectedCombo.hairTexture.name
            );
        }


        // =====================================================
        // KOMBİN DEBUG
        // =====================================================

        Debug.Log(
            $"{gameObject.name} kombin seçildi: " +
            (string.IsNullOrEmpty(selectedCombo.comboName)
                ? "(isimsiz)"
                : selectedCombo.comboName)
        );
    }


    // =========================================================
    // TEKRAR ETMEYEN RASTGELE KOMBİN
    // =========================================================

    private OutfitCombo SelectRandomCombo()
    {
        string characterKey =
            GetCharacterKey();


        if (!comboHistoryByCharacter.ContainsKey(
            characterKey))
        {
            comboHistoryByCharacter[characterKey] =
                new List<string>();
        }


        List<string> history =
            comboHistoryByCharacter[characterKey];


        int effectiveCooldown =
            Mathf.Clamp(
                comboCooldownCount,
                0,
                Mathf.Max(
                    0,
                    outfitCombos.Count - 1
                )
            );


        List<OutfitCombo> candidates =
            new List<OutfitCombo>();


        foreach (OutfitCombo combo in outfitCombos)
        {
            string key =
                GetComboKey(combo);


            if (!history.Contains(key))
            {
                candidates.Add(combo);
            }
        }


        if (candidates.Count == 0)
        {
            candidates =
                new List<OutfitCombo>(
                    outfitCombos
                );
        }


        OutfitCombo selected =
            candidates[
                UnityEngine.Random.Range(
                    0,
                    candidates.Count
                )
            ];


        history.Add(
            GetComboKey(selected)
        );


        while (
            history.Count >
            effectiveCooldown)
        {
            history.RemoveAt(0);
        }


        return selected;
    }


    // =========================================================
    // KARAKTER KİMLİĞİ
    // =========================================================

    private string GetCharacterKey()
    {
        return gameObject.name.Replace(
            "(Clone)",
            ""
        ).Trim();
    }


    // =========================================================
    // KOMBİN KİMLİĞİ
    // =========================================================

    private string GetComboKey(
        OutfitCombo combo)
    {
        if (!string.IsNullOrEmpty(
            combo.comboName))
        {
            return combo.comboName;
        }


        string clothes =
            combo.clothesTexture != null
                ? combo.clothesTexture.name
                : "null";


        string body =
            combo.bodyTexture != null
                ? combo.bodyTexture.name
                : "null";


        string hair =
            combo.hairTexture != null
                ? combo.hairTexture.name
                : "null";


        return $"{clothes}|{body}|{hair}";
    }
}