using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DayCycleManager'ın saatine (CurrentHour) göre Directional Light'ın
/// açısını, parlaklığını, rengini ve ambient ışık şiddetini anahtar
/// noktalar (keyframe) arasında yumuşak geçişle günceller.
///
/// Directional Light objesine eklenmesi en kolayı — Sun Light alanına
/// kendi Light component'ini sürüklersin.
/// </summary>
public class DayNightLightingController : MonoBehaviour
{
    [Serializable]
    public class LightingKeyframe
    {
        [Tooltip("Oyun-içi saat (örn. 9, 12, 17, 20).")]
        public float hour;

        [Tooltip("Directional Light'ın X rotasyonu. Yüksek = tepede (öğle), düşük/negatif = ufukta/altında (gece).")]
        public float sunAngleX = 45f;

        [Tooltip("Işık şiddeti.")]
        public float intensity = 1f;

        [Tooltip("Işık rengi (gün batımında turuncuya kayar).")]
        public Color color = Color.white;

        [Tooltip("Ortam (ambient) ışık çarpanı — RenderSettings.ambientIntensity'ye uygulanır. Ambient Source = Skybox olduğu için çalışır.")]
        [Range(0f, 1f)]
        public float ambientIntensity = 1f;
    }

    [Header("Referanslar")]
    [SerializeField] private Light sunLight;

    [Header("Sabit Eksenler (Değişmeyen Rotasyon)")]
    [Tooltip("Mevcut Directional Light'ın Y ve Z rotasyonuyla aynı tut, sadece X (yükseklik açısı) değişsin.")]
    [SerializeField] private float fixedYRotation = -30f;
    [SerializeField] private float fixedZRotation = 0f;

    [Header("Gün İçi Işık Anahtar Noktaları (Saate Göre Sıralı)")]
    [Tooltip("Senin tarif ettiğin akışa göre öntanımlı dolduruldu. Inspector'dan rengi/parlaklığı serbestçe ayarlayabilirsin.")]
    [SerializeField]
    private List<LightingKeyframe> keyframes = new List<LightingKeyframe>
    {
        new LightingKeyframe { hour = 9f,  sunAngleX = 15f, intensity = 0.6f,  color = new Color(1f, 0.95f, 0.85f),   ambientIntensity = 0.6f },
        new LightingKeyframe { hour = 12f, sunAngleX = 55f, intensity = 1.0f,  color = Color.white,                   ambientIntensity = 1.0f },
        new LightingKeyframe { hour = 15f, sunAngleX = 60f, intensity = 1.05f, color = Color.white,                   ambientIntensity = 1.0f },
        new LightingKeyframe { hour = 17f, sunAngleX = 30f, intensity = 0.9f,  color = new Color(1f, 0.75f, 0.5f),    ambientIntensity = 0.8f },
        new LightingKeyframe { hour = 18f, sunAngleX = 15f, intensity = 0.6f,  color = new Color(1f, 0.55f, 0.3f),    ambientIntensity = 0.5f },
        new LightingKeyframe { hour = 19f, sunAngleX = 5f,  intensity = 0.3f,  color = new Color(0.9f, 0.35f, 0.25f), ambientIntensity = 0.3f },
        new LightingKeyframe { hour = 20f, sunAngleX = -5f, intensity = 0.08f, color = new Color(0.25f, 0.3f, 0.5f),  ambientIntensity = 0.15f },
    };

    private void Reset()
    {
        if (sunLight == null)
        {
            sunLight = GetComponent<Light>();
        }
    }

    private void Update()
    {
        if (sunLight == null || DayCycleManager.Instance == null)
            return;

        ApplyLighting(DayCycleManager.Instance.CurrentHour);
    }

    private void ApplyLighting(float hour)
    {
        if (keyframes == null || keyframes.Count == 0)
            return;

        float clampedHour =
            Mathf.Clamp(
                hour,
                keyframes[0].hour,
                keyframes[keyframes.Count - 1].hour
            );

        LightingKeyframe before = keyframes[0];
        LightingKeyframe after = keyframes[keyframes.Count - 1];

        for (int i = 0; i < keyframes.Count - 1; i++)
        {
            if (clampedHour >= keyframes[i].hour &&
                clampedHour <= keyframes[i + 1].hour)
            {
                before = keyframes[i];
                after = keyframes[i + 1];
                break;
            }
        }

        float range = after.hour - before.hour;

        float t =
            range > 0.0001f
                ? Mathf.Clamp01((clampedHour - before.hour) / range)
                : 0f;

        t = Mathf.SmoothStep(0f, 1f, t);

        float angleX = Mathf.Lerp(before.sunAngleX, after.sunAngleX, t);
        float intensity = Mathf.Lerp(before.intensity, after.intensity, t);
        Color color = Color.Lerp(before.color, after.color, t);
        float ambient = Mathf.Lerp(before.ambientIntensity, after.ambientIntensity, t);

        sunLight.transform.rotation =
            Quaternion.Euler(angleX, fixedYRotation, fixedZRotation);

        sunLight.intensity = intensity;
        sunLight.color = color;

        RenderSettings.ambientIntensity = ambient;
    }
}