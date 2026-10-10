using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(CupPourReceiver))]
public class CupPourInteraction : MonoBehaviour, IHoldInteractable
{
    [Header("Ayarlar")]
    [SerializeField] private float holdDuration = 1.5f;

    [Header("UI")]
    [SerializeField]
    private string pourPrompt = "E'ye basılı tut";


    // =========================================================
    // SÜT SESİ
    // =========================================================

    [Header("Süt Dökme Sesi")]
    [SerializeField]
    private AudioClip milkPourClip;

    [SerializeField]
    [Range(0f, 1f)]
    private float milkPourVolume = 1f;


    // =========================================================
    // ESPRESSO SESİ
    // =========================================================

    [Header("Espresso Dökme Sesi")]
    [SerializeField]
    private AudioClip espressoPourClip;

    [SerializeField]
    [Range(0f, 1f)]
    private float espressoPourVolume = 1f;


    // =========================================================
    // SU SESİ
    // =========================================================

    [Header("Sıcak Su Dökme Sesi")]
    [SerializeField]
    private AudioClip waterPourClip;

    [SerializeField]
    [Range(0f, 1f)]
    private float waterPourVolume = 1f;


    // =========================================================
    // REFERANS
    // =========================================================

    private CupPourReceiver receiver;


    // =========================================================
    // SES
    // =========================================================

    private AudioSource runtimeAudioSource;

    private bool pourSoundPlaying = false;

    private AudioClip currentPourClip;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        receiver =
            GetComponent<CupPourReceiver>();

        CreateAudioSource();
    }


    // =========================================================
    // AUDIO SOURCE
    // =========================================================

    private void CreateAudioSource()
    {
        runtimeAudioSource =
            gameObject.AddComponent<AudioSource>();

        runtimeAudioSource.playOnAwake = false;

        runtimeAudioSource.loop = true;

        // 2D ses
        runtimeAudioSource.spatialBlend = 0f;

        runtimeAudioSource.mute = false;

        runtimeAudioSource.priority = 128;
    }


    // =========================================================
    // HOLD
    // =========================================================

    public float HoldDuration =>
        holdDuration;


    public string GetHoldPrompt()
    {
        return pourPrompt;
    }


    // =========================================================
    // CAN START HOLD
    // =========================================================

    public bool CanStartHold(
        PlayerInteraction player)
    {
        if (player == null)
            return false;

        if (receiver == null)
            return false;


        // =====================================================
        // SAĞ EL
        // =====================================================

        PickupItem held =
            player.GetHeldItem();

        if (held != null)
        {
            // -------------------------------------------------
            // ESPRESSO
            // -------------------------------------------------

            PourSource espressoSource =
                held.GetComponent<PourSource>();

            if (espressoSource != null &&
                espressoSource.HasEspresso &&
                espressoSource.gameObject != gameObject)
            {
                return receiver.CanReceiveEspresso(
                    espressoSource
                );
            }


            // -------------------------------------------------
            // DİREKT SÜT KUTUSU
            // -------------------------------------------------

            MilkSource milkSource =
                held.GetComponent<MilkSource>();

            if (milkSource != null)
            {
                return receiver.CanReceiveMilkSource(
                    milkSource
                );
            }


            // -------------------------------------------------
            // SÜT PITCHER
            // -------------------------------------------------

            MilkFiller milkFiller =
                held.GetComponent<MilkFiller>();

            if (milkFiller != null)
            {
                if (milkFiller.IsFrothed)
                {
                    return receiver.CanReceiveFrothedMilk(
                        milkFiller
                    );
                }

                return receiver.CanReceiveMilk(
                    milkFiller
                );
            }


            // -------------------------------------------------
            // KETTLE
            // -------------------------------------------------

            KettleWaterState kettleWater =
                held.GetComponent<KettleWaterState>();

            if (kettleWater != null)
            {
                return receiver.CanReceiveHotWater(
                    kettleWater
                );
            }
        }


        // =====================================================
        // SOL EL
        // =====================================================

        PickupItem leftHeld =
            player.GetLeftHeldItem();

        if (leftHeld != null)
        {
            // -------------------------------------------------
            // ESPRESSO
            // -------------------------------------------------

            PourSource espressoSource =
                leftHeld.GetComponent<PourSource>();

            if (espressoSource != null &&
                espressoSource.HasEspresso &&
                espressoSource.gameObject != gameObject)
            {
                return receiver.CanReceiveEspresso(
                    espressoSource
                );
            }


            // -------------------------------------------------
            // DİREKT SÜT KUTUSU
            // -------------------------------------------------

            MilkSource milkSource =
                leftHeld.GetComponent<MilkSource>();

            if (milkSource != null)
            {
                return receiver.CanReceiveMilkSource(
                    milkSource
                );
            }


            // -------------------------------------------------
            // SÜT PITCHER
            // -------------------------------------------------

            MilkFiller milkFiller =
                leftHeld.GetComponent<MilkFiller>();

            if (milkFiller != null)
            {
                if (milkFiller.IsFrothed)
                {
                    return receiver.CanReceiveFrothedMilk(
                        milkFiller
                    );
                }

                return receiver.CanReceiveMilk(
                    milkFiller
                );
            }


            // -------------------------------------------------
            // KETTLE
            // -------------------------------------------------

            KettleWaterState kettleWater =
                leftHeld.GetComponent<KettleWaterState>();

            if (kettleWater != null)
            {
                return receiver.CanReceiveHotWater(
                    kettleWater
                );
            }
        }


        return false;
    }


    // =========================================================
    // HOLD PROGRESS
    // =========================================================

    public void OnHoldProgress(
        PlayerInteraction player,
        float progress01)
    {
        if (player == null)
            return;

        if (receiver == null)
            return;


        // =====================================================
        // SAĞ EL
        // =====================================================

        PickupItem held =
            player.GetHeldItem();

        if (held != null)
        {
            // -------------------------------------------------
            // ESPRESSO
            // -------------------------------------------------

            PourSource espressoSource =
                held.GetComponent<PourSource>();

            if (espressoSource != null &&
                espressoSource.HasEspresso &&
                espressoSource.gameObject != gameObject)
            {
                StartEspressoPourSound();

                receiver.SetEspressoProgress(
                    progress01
                );

                return;
            }


            // -------------------------------------------------
            // DİREKT SÜT
            // -------------------------------------------------

            MilkSource milkSource =
                held.GetComponent<MilkSource>();

            if (milkSource != null)
            {
                StartMilkPourSound();

                receiver.SetMilkSourceProgress(
                    progress01
                );

                return;
            }


            // -------------------------------------------------
            // SÜT PITCHER
            // -------------------------------------------------

            MilkFiller milkFiller =
                held.GetComponent<MilkFiller>();

            if (milkFiller != null)
            {
                StartMilkPourSound();

                if (milkFiller.IsFrothed)
                {
                    receiver.SetFrothedMilkProgress(
                        progress01
                    );
                }
                else
                {
                    receiver.SetMilkProgress(
                        progress01
                    );
                }

                return;
            }


            // -------------------------------------------------
            // KETTLE
            // -------------------------------------------------

            KettleWaterState kettleWater =
                held.GetComponent<KettleWaterState>();

            if (kettleWater != null)
            {
                StartWaterPourSound();

                receiver.SetWaterProgress(
                    progress01
                );

                return;
            }
        }


        // =====================================================
        // SOL EL
        // =====================================================

        PickupItem leftHeld =
            player.GetLeftHeldItem();

        if (leftHeld != null)
        {
            // -------------------------------------------------
            // ESPRESSO
            // -------------------------------------------------

            PourSource espressoSource =
                leftHeld.GetComponent<PourSource>();

            if (espressoSource != null &&
                espressoSource.HasEspresso &&
                espressoSource.gameObject != gameObject)
            {
                StartEspressoPourSound();

                receiver.SetEspressoProgress(
                    progress01
                );

                return;
            }


            // -------------------------------------------------
            // DİREKT SÜT
            // -------------------------------------------------

            MilkSource milkSource =
                leftHeld.GetComponent<MilkSource>();

            if (milkSource != null)
            {
                StartMilkPourSound();

                receiver.SetMilkSourceProgress(
                    progress01
                );

                return;
            }


            // -------------------------------------------------
            // SÜT PITCHER
            // -------------------------------------------------

            MilkFiller milkFiller =
                leftHeld.GetComponent<MilkFiller>();

            if (milkFiller != null)
            {
                StartMilkPourSound();

                if (milkFiller.IsFrothed)
                {
                    receiver.SetFrothedMilkProgress(
                        progress01
                    );
                }
                else
                {
                    receiver.SetMilkProgress(
                        progress01
                    );
                }

                return;
            }


            // -------------------------------------------------
            // KETTLE
            // -------------------------------------------------

            KettleWaterState kettleWater =
                leftHeld.GetComponent<KettleWaterState>();

            if (kettleWater != null)
            {
                StartWaterPourSound();

                receiver.SetWaterProgress(
                    progress01
                );

                return;
            }
        }
    }


    // =========================================================
    // HOLD COMPLETE
    // =========================================================

    public void OnHoldComplete(
        PlayerInteraction player)
    {
        if (player == null)
        {
            StopPourSound();
            return;
        }

        if (receiver == null)
        {
            StopPourSound();
            return;
        }


        // =====================================================
        // SAĞ EL
        // =====================================================

        PickupItem held =
            player.GetHeldItem();

        if (held != null)
        {
            // -------------------------------------------------
            // ESPRESSO
            // -------------------------------------------------

            PourSource espressoSource =
                held.GetComponent<PourSource>();

            if (espressoSource != null &&
                espressoSource.HasEspresso &&
                espressoSource.gameObject != gameObject)
            {
                receiver.ReceiveEspresso(
                    espressoSource
                );

                StopPourSound();

                return;
            }


            // -------------------------------------------------
            // DİREKT SÜT
            // -------------------------------------------------

            MilkSource milkSource =
                held.GetComponent<MilkSource>();

            if (milkSource != null)
            {
                receiver.ReceiveMilkSource(
                    milkSource
                );

                StopPourSound();

                return;
            }


            // -------------------------------------------------
            // SÜT PITCHER
            // -------------------------------------------------

            MilkFiller milkFiller =
                held.GetComponent<MilkFiller>();

            if (milkFiller != null)
            {
                if (milkFiller.IsFrothed)
                {
                    receiver.ReceiveFrothedMilk(
                        milkFiller
                    );
                }
                else
                {
                    receiver.ReceiveMilk(
                        milkFiller
                    );
                }

                StopPourSound();

                return;
            }


            // -------------------------------------------------
            // KETTLE
            // -------------------------------------------------

            KettleWaterState kettleWater =
                held.GetComponent<KettleWaterState>();

            if (kettleWater != null)
            {
                receiver.ReceiveHotWater(
                    kettleWater
                );

                StopPourSound();

                return;
            }
        }


        // =====================================================
        // SOL EL
        // =====================================================

        PickupItem leftHeld =
            player.GetLeftHeldItem();

        if (leftHeld != null)
        {
            // -------------------------------------------------
            // ESPRESSO
            // -------------------------------------------------

            PourSource espressoSource =
                leftHeld.GetComponent<PourSource>();

            if (espressoSource != null &&
                espressoSource.HasEspresso &&
                espressoSource.gameObject != gameObject)
            {
                receiver.ReceiveEspresso(
                    espressoSource
                );

                StopPourSound();

                return;
            }


            // -------------------------------------------------
            // DİREKT SÜT
            // -------------------------------------------------

            MilkSource milkSource =
                leftHeld.GetComponent<MilkSource>();

            if (milkSource != null)
            {
                receiver.ReceiveMilkSource(
                    milkSource
                );

                StopPourSound();

                return;
            }


            // -------------------------------------------------
            // SÜT PITCHER
            // -------------------------------------------------

            MilkFiller milkFiller =
                leftHeld.GetComponent<MilkFiller>();

            if (milkFiller != null)
            {
                if (milkFiller.IsFrothed)
                {
                    receiver.ReceiveFrothedMilk(
                        milkFiller
                    );
                }
                else
                {
                    receiver.ReceiveMilk(
                        milkFiller
                    );
                }

                StopPourSound();

                return;
            }


            // -------------------------------------------------
            // KETTLE
            // -------------------------------------------------

            KettleWaterState kettleWater =
                leftHeld.GetComponent<KettleWaterState>();

            if (kettleWater != null)
            {
                receiver.ReceiveHotWater(
                    kettleWater
                );

                StopPourSound();

                return;
            }
        }


        StopPourSound();
    }


    // =========================================================
    // SÜT SESİ
    // =========================================================

    private void StartMilkPourSound()
    {
        if (milkPourClip == null)
        {
            Debug.LogError(
                "MILK POUR CLIP BOS! " +
                gameObject.name
            );

            return;
        }


        PlayPourSound(
            milkPourClip,
            milkPourVolume
        );


        Debug.Log(
            "SUT DOKME SESI BASLADI! " +
            gameObject.name
        );
    }


    // =========================================================
    // ESPRESSO SESİ
    // =========================================================

    private void StartEspressoPourSound()
    {
        if (espressoPourClip == null)
        {
            Debug.LogError(
                "ESPRESSO POUR CLIP BOS! " +
                gameObject.name
            );

            return;
        }


        PlayPourSound(
            espressoPourClip,
            espressoPourVolume
        );


        Debug.Log(
            "ESPRESSO DOKME SESI BASLADI! " +
            gameObject.name
        );
    }


    // =========================================================
    // SU SESİ
    // =========================================================

    private void StartWaterPourSound()
    {
        if (waterPourClip == null)
        {
            Debug.LogError(
                "WATER POUR CLIP BOS! " +
                gameObject.name
            );

            return;
        }


        PlayPourSound(
            waterPourClip,
            waterPourVolume
        );


        Debug.Log(
            "SICAK SU DOKME SESI BASLADI! " +
            gameObject.name
        );
    }


    // =========================================================
    // ORTAK SES
    // =========================================================

    private void PlayPourSound(
        AudioClip clip,
        float volume)
    {
        if (runtimeAudioSource == null)
        {
            CreateAudioSource();
        }


        // Aynı ses zaten çalıyorsa tekrar başlatma.
        if (pourSoundPlaying &&
            currentPourClip == clip)
        {
            return;
        }


        // Başka dökme sesi çalıyorsa durdur.
        if (runtimeAudioSource.isPlaying)
        {
            runtimeAudioSource.Stop();
        }


        currentPourClip =
            clip;


        runtimeAudioSource.clip =
            clip;

        runtimeAudioSource.volume =
            volume;

        runtimeAudioSource.loop = true;

        runtimeAudioSource.spatialBlend = 0f;

        runtimeAudioSource.mute = false;


        runtimeAudioSource.Play();

        pourSoundPlaying = true;
    }


    // =========================================================
    // SESİ DURDUR
    // =========================================================

    private void StopPourSound()
    {
        pourSoundPlaying = false;

        currentPourClip = null;


        if (runtimeAudioSource == null)
            return;


        if (runtimeAudioSource.isPlaying)
        {
            runtimeAudioSource.Stop();

            Debug.Log(
                "DOKME SESI DURDU! " +
                gameObject.name
            );
        }
    }


    // =========================================================
    // HOLD İPTAL
    // =========================================================

    public void CancelHold()
    {
        StopPourSound();
    }


    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        StopPourSound();
    }
}