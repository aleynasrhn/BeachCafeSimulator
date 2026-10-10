using UnityEngine;

/// <summary>
/// Coffee grinder objesine eklenir. Oyuncu elinde BOŞ portafilter varken buraya
/// bakıp E'yi holdDuration kadar basılı tutunca içi kahveyle dolar.
///
/// Hold başladığında grinder sesi çalar.
/// E erken bırakılırsa ses durur.
/// Hedef değiştirilirse ses durur.
/// İşlem tamamlandığında kahve doldurulur ve ses durur.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CoffeeGrinderInteraction : MonoBehaviour, IHoldInteractable
{
    [Header("Grinder Ayarları")]
    [SerializeField] private float holdDuration = 3.5f;
    [SerializeField] private string acceptedItemName = "portafilter";

    [Header("Ses")]
    [SerializeField] private AudioSource grinderAudioSource;

    public float HoldDuration => holdDuration;

    public string GetHoldPrompt()
    {
        return "E'ye basılı tut";
    }

    public bool CanStartHold(PlayerInteraction player)
    {
        if (player == null)
            return false;

        PickupItem held = player.GetHeldItem();

        if (held == null)
            return false;

        if (held.ItemName != acceptedItemName)
            return false;

        if (held.HasGroundCoffee)
            return false;

        return true;
    }

    public void OnHoldProgress(
        PlayerInteraction player,
        float progress01)
    {
        // E'ye basılı tutulmaya başlandığında sesi başlat.
        if (grinderAudioSource != null &&
            progress01 > 0f &&
            !grinderAudioSource.isPlaying)
        {
            grinderAudioSource.Play();
        }
    }

    public void OnHoldComplete(
        PlayerInteraction player)
    {
        if (player == null)
        {
            StopGrinderSound();
            return;
        }

        PickupItem held =
            player.GetHeldItem();

        if (held == null)
        {
            StopGrinderSound();
            return;
        }

        held.FillWithGroundCoffee();

        StopGrinderSound();
    }

    // =========================================================
    // HOLD İPTAL
    // =========================================================

    public void CancelHold()
    {
        StopGrinderSound();
    }

    // =========================================================
    // SESİ DURDUR
    // =========================================================

    private void StopGrinderSound()
    {
        if (grinderAudioSource != null)
        {
            grinderAudioSource.Stop();
        }
    }

    // =========================================================
    // OBJE KAPATILIRSA
    // =========================================================

    private void OnDisable()
    {
        StopGrinderSound();
    }
}