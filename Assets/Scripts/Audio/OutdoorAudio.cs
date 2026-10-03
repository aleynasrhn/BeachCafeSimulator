using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class OutdoorAudio : MonoBehaviour
{
    [Header("Ses Kaynakları")]
    [SerializeField] private AudioSource windAudioSource;
    [SerializeField] private AudioSource seaAudioSource;

    [Header("Ses Seviyeleri")]
    [SerializeField, Range(0f, 1f)] private float windVolume = 0.3f;
    [SerializeField, Range(0f, 1f)] private float seaVolume = 0.25f;

    [Header("Zemin Kontrolü")]
    [SerializeField] private LayerMask cafeFloorLayer;
    [SerializeField] private float groundCheckDistance = 2f;

    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        SetupAudioSource(windAudioSource, windVolume);
        SetupAudioSource(seaAudioSource, seaVolume);
    }

    private void Update()
    {
        CheckOutdoorAudio();
    }

    private void SetupAudioSource(
        AudioSource source,
        float volume)
    {
        if (source == null)
            return;

        source.playOnAwake = false;
        source.loop = true;
        source.volume = volume;
        source.spatialBlend = 0f;
    }

    private void CheckOutdoorAudio()
    {
        Vector3 rayOrigin =
            transform.position + Vector3.up * 0.2f;

        bool isOnCafeFloor = Physics.Raycast(
            rayOrigin,
            Vector3.down,
            groundCheckDistance,
            cafeFloorLayer
        );

        bool isGrounded = controller.isGrounded;

        bool shouldPlayOutdoorAudio =
            isGrounded &&
            !isOnCafeFloor;

        if (shouldPlayOutdoorAudio)
        {
            StartAudio(windAudioSource);
            StartAudio(seaAudioSource);
        }
        else
        {
            StopAudio(windAudioSource);
            StopAudio(seaAudioSource);
        }
    }

    private void StartAudio(AudioSource source)
    {
        if (source != null && !source.isPlaying)
        {
            source.Play();
        }
    }

    private void StopAudio(AudioSource source)
    {
        if (source != null && source.isPlaying)
        {
            source.Stop();
        }
    }

    private void OnDisable()
    {
        StopAudio(windAudioSource);
        StopAudio(seaAudioSource);
    }
}