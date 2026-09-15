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

    private bool isOpen = false;

    // KettleWaterFillPoint bunu okuyacak
    public bool IsOpen => isOpen;


    // =========================================================
    // PROMPT
    // =========================================================

    public string GetInteractPrompt()
    {
        // Prompt göstermiyoruz.
        return "";
    }


    // =========================================================
    // ETKİLEŞİM
    // =========================================================

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

        // Açık ↔ kapalı
        isOpen = !isOpen;

        UpdateFaucet();
    }


    // =========================================================
    // MUSLUĞU GÜNCELLE
    // =========================================================

    private void UpdateFaucet()
    {
        if (isOpen)
        {
            // Kolu açık pozisyona döndür
            handle.localRotation =
                Quaternion.Euler(
                    openRotation
                );

            // Suyu göster
            if (waterVisual != null)
            {
                waterVisual.SetActive(true);
            }
        }
        else
        {
            // Kolu kapalı pozisyona döndür
            handle.localRotation =
                Quaternion.Euler(
                    closedRotation
                );

            // Suyu gizle
            if (waterVisual != null)
            {
                waterVisual.SetActive(false);
            }
        }
    }


    // =========================================================
    // BAŞLANGIÇ
    // =========================================================

    private void Start()
    {
        if (handle != null)
        {
            // Sahnedeki mevcut kapalı rotasyonu başlangıç değeri yap
            closedRotation =
                handle.localEulerAngles;

            // Başlangıçta kapalı
            handle.localRotation =
                Quaternion.Euler(
                    closedRotation
                );
        }

        isOpen = false;

        if (waterVisual != null)
        {
            waterVisual.SetActive(false);
        }
    }
}