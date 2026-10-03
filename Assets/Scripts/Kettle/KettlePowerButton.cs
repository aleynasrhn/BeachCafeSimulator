using UnityEngine;
using System.Collections;

public class KettlePowerButton : MonoBehaviour, IInteractable
{
    [Header("Kettle")]
    [SerializeField] private KettleHeatController kettleHeat;

    [Header("Power Tuşu Sesi")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip powerSound;
    [SerializeField] private float soundDelay = 3.5f;

    private bool isOn = false;
    private Coroutine soundCoroutine;


    public string GetInteractPrompt()
    {
        return "";
    }


    public void Interact(PlayerInteraction player)
    {
        if (kettleHeat == null)
        {
            Debug.LogWarning(
                "KettlePowerButton: KettleHeatController atanmadı.",
                this
            );

            return;
        }


        if (isOn)
        {
            isOn = false;

            kettleHeat.StopHeating();

            return;
        }


        bool started = kettleHeat.StartHeating();


        if (started)
        {
            isOn = true;

            soundCoroutine = StartCoroutine(
                PlaySoundAfterDelay()
            );
        }
    }


    private IEnumerator PlaySoundAfterDelay()
    {
        yield return new WaitForSeconds(soundDelay);

        if (audioSource != null && powerSound != null)
        {
            audioSource.loop = false;
            audioSource.PlayOneShot(powerSound);
        }

        soundCoroutine = null;
    }
}