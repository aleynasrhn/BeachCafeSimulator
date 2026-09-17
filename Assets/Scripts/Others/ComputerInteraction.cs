using UnityEngine;

public class ComputerInteraction : MonoBehaviour
{
    [Header("Kamera")]
    public Transform cameraHolder;
    public Transform computerCameraPoint;

    [Header("Oyuncu")]
    public PlayerMovement playerMovement;

    [Header("Ayarlar")]
    public float cameraMoveSpeed = 6f;

    private bool isUsingComputer = false;
    private bool isReturning = false;

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    public bool IsUsingComputer => isUsingComputer;


    // =========================================================
    // PC'YE GİR
    // =========================================================

    public void EnterComputer()
    {
        if (isUsingComputer || isReturning)
            return;

        // Oyuncunun mevcut kamera konumunu kaydet
        originalCameraPosition = cameraHolder.position;
        originalCameraRotation = cameraHolder.rotation;

        isUsingComputer = true;

        // Oyuncu hareket edemesin
        playerMovement.canMove = false;

        // Mouse'u serbest bırak
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }


    // =========================================================
    // PC'DEN ÇIK
    // =========================================================

    public void ExitComputer()
    {
        if (!isUsingComputer)
            return;

        // Artık PC kullanımı bitiyor
        isUsingComputer = false;

        // Geri dönüş başladı
        isReturning = true;

        // Kamera geri dönerken oyuncu hareket edemesin
        playerMovement.canMove = false;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // =====================================================
        // PC'YE GİRİŞ / PC'DE KAMERA
        // =====================================================

        if (isUsingComputer)
        {
            MoveCameraToComputer();

            // E ile çık
            if (Input.GetKeyDown(KeyCode.E))
            {
                ExitComputer();
                return;
            }

            // ESC ile çık
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitComputer();
                return;
            }
        }


        // =====================================================
        // PC'DEN GERİ DÖNÜŞ
        // =====================================================

        if (isReturning)
        {
            ReturnCameraToOriginalPosition();
        }
    }


    // =========================================================
    // PC KAMERASINA SMOOTH GİT
    // =========================================================

    private void MoveCameraToComputer()
    {
        cameraHolder.position = Vector3.Lerp(
            cameraHolder.position,
            computerCameraPoint.position,
            Time.deltaTime * cameraMoveSpeed
        );

        cameraHolder.rotation = Quaternion.Lerp(
            cameraHolder.rotation,
            computerCameraPoint.rotation,
            Time.deltaTime * cameraMoveSpeed
        );
    }


    // =========================================================
    // ESKİ KAMERA KONUMUNA SMOOTH DÖN
    // =========================================================

    private void ReturnCameraToOriginalPosition()
    {
        cameraHolder.position = Vector3.Lerp(
            cameraHolder.position,
            originalCameraPosition,
            Time.deltaTime * cameraMoveSpeed
        );

        cameraHolder.rotation = Quaternion.Lerp(
            cameraHolder.rotation,
            originalCameraRotation,
            Time.deltaTime * cameraMoveSpeed
        );


        // Kamera yeterince yaklaştığında tamamen eski konuma oturt
        float distance =
            Vector3.Distance(
                cameraHolder.position,
                originalCameraPosition
            );

        float angle =
            Quaternion.Angle(
                cameraHolder.rotation,
                originalCameraRotation
            );


        if (distance < 0.01f && angle < 0.1f)
        {
            cameraHolder.position = originalCameraPosition;
            cameraHolder.rotation = originalCameraRotation;

            isReturning = false;

            // Oyuncuyu tekrar hareket ettir
            playerMovement.canMove = true;

            // Mouse'u tekrar kilitle
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}