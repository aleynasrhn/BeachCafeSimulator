using UnityEngine;

/// <summary>
/// Oyuncunun raycast ile etkileşime girebileceği her şey bu interface'i implement eder.
/// Sadece "ne yapılacağını" tanımlar, "nasıl yapılacağı" ilgili sınıfa kalır.
/// </summary>
public interface IInteractable
{
    // Oyuncu E'ye bastığında çağrılır
    void Interact(PlayerInteraction player);

    // UI'da "E - Al" gibi bir prompt göstermek için
    string GetInteractPrompt();
}