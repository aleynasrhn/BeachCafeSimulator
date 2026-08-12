using UnityEngine;

/// <summary>
/// Ekstra Espresso/Çikolata Şurubu/Karamel Şurup/Tarçın/Vanilya Şurubu
/// butonlarının HER BİRİNE eklenir.
/// </summary>
public class ExtraButtonUI : MonoBehaviour
{
    [SerializeField] private string extraName = "Ekstra Espresso";
    [SerializeField] private float price = 2f;
    [SerializeField] private bool locked = false;

    public string ExtraName => extraName;
    public float Price => price;
    public bool IsLocked => locked;
}