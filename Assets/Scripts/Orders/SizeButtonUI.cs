using UnityEngine;

/// <summary>
/// "Büyük" / "Orta" / "Small" boyut butonlarının HER BİRİNE eklenir.
/// </summary>
public class SizeButtonUI : MonoBehaviour
{
    [SerializeField] private CupSize size;
    public CupSize Size => size;
}