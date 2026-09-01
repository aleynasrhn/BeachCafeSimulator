using UnityEngine;

public class KettlePowerButton : MonoBehaviour, IInteractable
{
    [Header("Kettle")]
    [SerializeField] private KettleHeatController kettleHeat;

    private bool isOn = false;


    // =========================================================
    // PROMPT
    // =========================================================

    public string GetInteractPrompt()
    {
        return "";
    }


    // =========================================================
    // ETKİLEŞİM
    // =========================================================

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


        // =====================================================
        // AÇIKSA → KAPAT
        // =====================================================

        if (isOn)
        {
            isOn = false;

            kettleHeat.StopHeating();

            return;
        }


        // =====================================================
        // KAPALIYSA → BAŞLATMAYI DENE
        // =====================================================

        bool started =
            kettleHeat.StartHeating();


        // Gerçekten başladıysa ON
        if (started)
        {
            isOn = true;
        }
    }
}