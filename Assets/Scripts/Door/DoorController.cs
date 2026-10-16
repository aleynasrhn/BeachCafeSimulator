using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Kapı Parçaları")]
    [SerializeField] private DoorPanel[] doorPanels;

    [Header("Kapanma Gecikmesi")]
    [SerializeField] private float closeDelay = 4f;

    [Header("Kapı Sesi")]
    [SerializeField] private AudioSource doorAudioSource;
    [SerializeField] private AudioClip doorSound;
    [SerializeField] private float doorSoundVolume = 1f;

    private int customersInZone = 0;

    private Coroutine closeCoroutine;

    private bool doorsAreOpen = false;

    // =========================================================
    // MÜŞTERİ BÖLGEYE GİRDİ
    // =========================================================

    public void NotifyCustomerEntered()
    {
        customersInZone++;

        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }

        OpenDoors();

        Debug.Log(
            $"Kapı bölgesinde müşteri sayısı: {customersInZone} | Kapı açılıyor."
        );
    }

    // =========================================================
    // MÜŞTERİ BÖLGEDEN ÇIKTI
    // =========================================================

    public void NotifyCustomerExited()
    {
        customersInZone--;

        if (customersInZone < 0)
        {
            customersInZone = 0;
        }

        if (customersInZone == 0)
        {
            if (closeCoroutine != null)
            {
                StopCoroutine(closeCoroutine);
            }

            closeCoroutine = StartCoroutine(CloseAfterDelay());
        }

        Debug.Log(
            $"Kapı bölgesinde müşteri sayısı: {customersInZone}"
        );
    }

    // =========================================================
    // GECİKMELİ KAPANMA
    // =========================================================

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);

        CloseDoors();

        closeCoroutine = null;
    }

    // =========================================================
    // KAPILARI AÇ
    // =========================================================

    private void OpenDoors()
    {
        foreach (DoorPanel panel in doorPanels)
        {
            if (panel != null)
            {
                panel.MoveToOpen();
            }
        }

        // Kapı zaten açıksa tekrar ses çalma
        if (!doorsAreOpen)
        {
            PlayDoorSound();
            doorsAreOpen = true;
        }
    }

    // =========================================================
    // KAPILARI KAPAT
    // =========================================================

    private void CloseDoors()
    {
        foreach (DoorPanel panel in doorPanels)
        {
            if (panel != null)
            {
                panel.MoveToClosed();
            }
        }

        // Kapı zaten kapalıysa tekrar ses çalma
        if (doorsAreOpen)
        {
            PlayDoorSound();
            doorsAreOpen = false;
        }
    }

    // =========================================================
    // KAPI SESİNİ ÇAL
    // =========================================================

    private void PlayDoorSound()
    {
        if (doorAudioSource == null || doorSound == null)
        {
            Debug.LogWarning("Kapı AudioSource veya Door Sound atanmadı!");
            return;
        }

        doorAudioSource.PlayOneShot(doorSound, doorSoundVolume);
    }
}