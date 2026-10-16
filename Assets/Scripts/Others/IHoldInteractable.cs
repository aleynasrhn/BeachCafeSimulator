/// <summary>
/// Tek E basışıyla değil, E'yi bir süre BASILI TUTARAK tetiklenen etkileşimler için.
/// Coffee grinder gibi "işlem zaman alır" mekanikleri bunu implement eder.
/// </summary>
public interface IHoldInteractable
{
    float HoldDuration { get; }

    void OnHoldComplete(PlayerInteraction player);

    // Basılı tutma sırasında HER FRAME çağrılır (progress01: 0'dan 1'e) - animasyon için
    void OnHoldProgress(PlayerInteraction player, float progress01);

    string GetHoldPrompt();

    bool CanStartHold(PlayerInteraction player);
}