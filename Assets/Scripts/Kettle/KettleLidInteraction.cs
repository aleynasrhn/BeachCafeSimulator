using UnityEngine;

public class KettleLidInteraction : MonoBehaviour, IInteractable
{
    [Header("Kapak Pivotu")]
    [SerializeField] private Transform lidPoint;

    [Header("Kapalı Rotasyon")]
    [SerializeField]
    private Vector3 closedRotation =
        new Vector3(0f, 90f, 0f);

    [Header("Açık Rotasyon")]
    [SerializeField]
    private Vector3 openRotation =
        new Vector3(-60f, 90f, 0f);

    // Kapak açık mı?
    private bool isOpen = false;

    // Diğer scriptlerin okuyabilmesi için
    public bool IsOpen => isOpen;


    // =========================================================
    // PROMPT
    // =========================================================

    public string GetInteractPrompt()
    {
        // Ekranda ekstra prompt göstermiyoruz.
        return "";
    }


    // =========================================================
    // ETKİLEŞİM
    // =========================================================

    public void Interact(PlayerInteraction player)
    {
        if (lidPoint == null)
        {
            Debug.LogWarning(
                "KettleLidInteraction: LidPoint atanmadı!",
                this
            );

            return;
        }

        // Açık <-> kapalı
        isOpen = !isOpen;


        // =====================================================
        // AÇ
        // =====================================================

        if (isOpen)
        {
            lidPoint.localRotation =
                Quaternion.Euler(
                    openRotation
                );
        }


        // =====================================================
        // KAPAT
        // =====================================================

        else
        {
            lidPoint.localRotation =
                Quaternion.Euler(
                    closedRotation
                );
        }
    }


    // =========================================================
    // BAŞLANGIÇ
    // =========================================================

    private void Start()
    {
        isOpen = false;

        if (lidPoint != null)
        {
            lidPoint.localRotation =
                Quaternion.Euler(
                    closedRotation
                );
        }
    }
}