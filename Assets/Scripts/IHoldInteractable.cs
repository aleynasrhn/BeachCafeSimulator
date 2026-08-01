/// <summary>
/// Tek E basışıyla değil, E'yi bir süre BASILI TUTARAK tetiklenen etkileşimler için.
/// Coffee grinder gibi "işlem zaman alır" mekanikleri bunu implement eder.
/// </summary>
public interface IHoldInteractable
{
    // Kaç saniye basılı tutulması gerektiği
    float HoldDuration { get; }

    // Basılı tutma tamamlanınca çağrılır
    void OnHoldComplete(PlayerInteraction player);

    // UI'da gösterilecek metin (örn: "E - Kahve Öğüt")
    string GetHoldPrompt();

    // Şu an bu etkileşime başlanabilir mi? (Örn: elinde doğru item yoksa, ya da
    // zaten tamamlanmışsa false döner - hem prompt gösterilmez hem basılı tutma saymaz)
    bool CanStartHold(PlayerInteraction player);
}