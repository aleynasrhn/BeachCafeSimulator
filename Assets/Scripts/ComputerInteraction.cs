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

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    public bool IsUsingComputer => isUsingComputer;

    public void EnterComputer()
    {
        if (isUsingComputer)
            return;

        // Mevcut kamera konumunu kaydet
        originalCameraPosition = cameraHolder.position;
        originalCameraRotation = cameraHolder.rotation;

        isUsingComputer = true;

        // Oyuncunun hareketini kapat
        playerMovement.canMove = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ExitComputer()
    {
        if (!isUsingComputer)
            return;

        isUsingComputer = false;

        // Oyuncunun hareketini tekrar aç
        playerMovement.canMove = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (!isUsingComputer)
            return;

        // Kamerayı PC ekranına götür
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

        // E'ye basınca çık
        if (Input.GetKeyDown(KeyCode.E))
        {
            ExitComputer();
        }

        // ESC ile de çıkabilsin
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitComputer();
        }
    }
}