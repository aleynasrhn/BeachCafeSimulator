using UnityEngine;
using TMPro;

/// <summary>
/// Tepede zaten eklediğin "09:00" yazan TMP_Text objesine
/// ekle, Clock Text alanına o text'i sürükle.
/// </summary>
public class DayClockUI : MonoBehaviour
{
    [SerializeField] private TMP_Text clockText;

    private void Update()
    {
        if (DayCycleManager.Instance == null || clockText == null)
            return;

        clockText.text = DayCycleManager.Instance.GetTimeString();
    }
}