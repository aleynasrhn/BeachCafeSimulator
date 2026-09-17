using UnityEngine;

public class FaucetInteraction : MonoBehaviour, IInteractable
{
    [Header("Musluk Kolu")]
    [SerializeField] private Transform handle;

    [Tooltip("Musluk kapalıyken kullanılacak local rotation")]
    [SerializeField] private Vector3 closedRotation;

    [Tooltip("Musluk açıkken kullanılacak local rotation")]
    [SerializeField] private Vector3 openRotation;

    [Header("Su")]
    [SerializeField] private GameObject waterVisual;

    [Header("Musluk Sesi")]
    [SerializeField] private AudioSource faucetAudioSource;

    private bool isOpen = false;

    public bool IsOpen => isOpen;


    public string GetInteractPrompt()
    {
        return "";
    }


    public void Interact(PlayerInteraction player)
    {
        if (handle == null)
        {
            Debug.LogWarning(
                "FaucetInteraction: Handle atanmadı!",
                this
            );

            return;
        }

        isOpen = !isOpen;

        UpdateFaucet();
    }


    private void UpdateFaucet()
    {
        if (isOpen)
        {
            handle.localRotation =
                Quaternion.Euler(openRotation);

            if (waterVisual != null)
            {
                waterVisual.SetActive(true);
            }

            // Musluk sesi başlar
            if (faucetAudioSource != null &&
                !faucetAudioSource.isPlaying)
            {
                faucetAudioSource.loop = true;
                faucetAudioSource.Play();
            }
        }
        else
        {
            handle.localRotation =
                Quaternion.Euler(closedRotation);

            if (waterVisual != null)
            {
                waterVisual.SetActive(false);
            }

            // Musluk sesi durur
            if (faucetAudioSource != null)
            {
                faucetAudioSource.Stop();
            }
        }
    }


    private void Start()
    {
        if (handle != null)
        {
            closedRotation =
                handle.localEulerAngles;

            handle.localRotation =
                Quaternion.Euler(closedRotation);
        }

        isOpen = false;

        if (waterVisual != null)
        {
            waterVisual.SetActive(false);
        }

        if (faucetAudioSource != null)
        {
            faucetAudioSource.loop = true;
            faucetAudioSource.Stop();
        }
    }
}