using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Servis hızına göre bahşiş miktarını hesaplar.
/// timeRatio = müşterinin masada beklediği süre / sipariş bekleme süresi
/// (0 = anında servis, 1 = süre dolmak üzere).
/// </summary>
public class TipManager : MonoBehaviour
{
    public static TipManager Instance { get; private set; }

    [Serializable]
    public class TipTier
    {
        [Tooltip("Süre oranı bu değere kadar (dahil) ise bu kademe geçerli.")]
        [Range(0f, 1f)]
        public float maxTimeRatio = 0.5f;

        public float minTip = 1f;
        public float maxTip = 1.5f;
    }

    [Header("Bahşiş Kademeleri")]
    [Tooltip("Listedeki hiçbir kademeye uymayan (en yüksek maxTimeRatio'dan sonraki) servis bahşiş getirmez.")]
    [SerializeField]
    private List<TipTier> tiers = new List<TipTier>
    {
        new TipTier { maxTimeRatio = 0.50f, minTip = 1.00f, maxTip = 1.50f },
        new TipTier { maxTimeRatio = 0.75f, minTip = 0.50f, maxTip = 1.00f },
        new TipTier { maxTimeRatio = 0.90f, minTip = 0.25f, maxTip = 0.50f },
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public float CalculateTip(float timeRatio)
    {
        TipTier best = null;

        foreach (TipTier tier in tiers)
        {
            if (tier == null)
                continue;

            if (timeRatio <= tier.maxTimeRatio &&
                (best == null || tier.maxTimeRatio < best.maxTimeRatio))
            {
                best = tier;
            }
        }

        if (best == null)
            return 0f;

        float amount =
            UnityEngine.Random.Range(best.minTip, best.maxTip);

        return Mathf.Round(amount * 100f) / 100f;
    }
}