using UnityEngine;

public class StreetLightController : MonoBehaviour
{
    [Header("Sokak Lambaları")]
    [SerializeField] private Light[] streetLights;

    [Header("Açılma Saati")]
    [SerializeField] private float turnOnHour = 18f;

    [Header("Kapanma Saati")]
    [SerializeField] private float turnOffHour = 6f;

    [Header("Işık Şiddeti")]
    [SerializeField] private float lightIntensity = 3f;

    private void Start()
    {
        UpdateStreetLights();
    }

    private void Update()
    {
        if (DayCycleManager.Instance == null)
            return;

        UpdateStreetLights();
    }

    private void UpdateStreetLights()
    {
        float currentHour =
            DayCycleManager.Instance.CurrentHour;

        bool shouldBeOn;

        // 18:00 - 24:00
        if (currentHour >= turnOnHour)
        {
            shouldBeOn = true;
        }
        // 00:00 - 06:00
        else if (currentHour < turnOffHour)
        {
            shouldBeOn = true;
        }
        else
        {
            shouldBeOn = false;
        }

        if (streetLights == null)
            return;

        foreach (Light light in streetLights)
        {
            if (light == null)
                continue;

            light.enabled = shouldBeOn;
            light.intensity = lightIntensity;
        }
    }
}