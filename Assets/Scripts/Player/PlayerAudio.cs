using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudio : MonoBehaviour
{
    [Header("Ses Ayarları")]
    [SerializeField, Range(0f, 1f)] private float walkingVolume = 0.5f;
    [SerializeField] private float walkingPitch = 1f;
    [SerializeField] private float runningPitch = 1.35f;

    private AudioSource audioSource;
    private CharacterController controller;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        controller = GetComponent<CharacterController>();

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.volume = walkingVolume;
        audioSource.spatialBlend = 0f;
        audioSource.pitch = walkingPitch;
    }

    private void Update()
    {
        if (controller == null)
            return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        bool isMoving =
            horizontal != 0f ||
            vertical != 0f;

        bool isGrounded = controller.isGrounded;

        bool isRunning =
            Input.GetKey(KeyCode.LeftShift) &&
            isMoving;

        bool shouldPlayFootsteps =
            isMoving &&
            isGrounded;

        // Yürüme ve koşma ses hızını değiştir
        float targetPitch = isRunning
            ? runningPitch
            : walkingPitch;

        audioSource.pitch = Mathf.Lerp(
            audioSource.pitch,
            targetPitch,
            Time.deltaTime * 8f
        );

        if (shouldPlayFootsteps)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    private void OnDisable()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}