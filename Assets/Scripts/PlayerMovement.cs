using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public CharacterController controller;
    public Transform cameraHolder;

    [Header("Movement")]

    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float acceleration = 10f;
    public float mouseSensitivity = 200f;
    public float gravity = -30f;
  
    private float currentSpeed;
    private float xRotation;

    private Vector3 velocity;
    private bool isGrounded;

    [Header("HeadBob")]
    public float bobSpeed = 12f;
    public float bobAmount = 0.05f;

    private float defaultYPos;
    private float timer;

    [Header("Camera")]

    public Camera playerCamera;

    public float walkFOV = 60f;
    public float runFOV = 70f;
    public float fovSpeed = 8f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        defaultYPos = cameraHolder.localPosition.y;
        playerCamera.fieldOfView = walkFOV;
    }

    void Update()
    {
        MovePlayer();
        LookAround();
        ApplyGravity();
        HeadBob();
        UpdateFOV();
    }

  
    void MovePlayer()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = transform.right * horizontal +
                            transform.forward * vertical;

        direction.Normalize();

        float targetSpeed = Input.GetKey(KeyCode.LeftShift)
            ? runSpeed
            : walkSpeed;

        if (direction.magnitude > 0)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, acceleration * Time.deltaTime);
        }

        controller.Move(direction * currentSpeed * Time.deltaTime);
    }

    void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

  
    void ApplyGravity()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -5f;
        }


        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    void HeadBob()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Hareket etmiyorsa veya havadaysa
        if (!controller.isGrounded || (horizontal == 0 && vertical == 0))
        {
            Vector3 pos = cameraHolder.localPosition;
            pos.y = Mathf.Lerp(pos.y, defaultYPos, Time.deltaTime * 10f);
            cameraHolder.localPosition = pos;
            return;
        }

        float currentBobSpeed = Input.GetKey(KeyCode.LeftShift)
            ? bobSpeed * 1.5f
            : bobSpeed;

        float currentBobAmount = Input.GetKey(KeyCode.LeftShift)
            ? bobAmount * 1.3f
            : bobAmount;

        timer += Time.deltaTime * currentBobSpeed;

        Vector3 newPos = cameraHolder.localPosition;
        newPos.y = defaultYPos + Mathf.Sin(timer) * currentBobAmount;

        cameraHolder.localPosition = newPos;
    }

    void UpdateFOV()
    {
        float targetFOV = Input.GetKey(KeyCode.LeftShift)
            ? runFOV
            : walkFOV;

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            fovSpeed * Time.deltaTime
        );
    }

}