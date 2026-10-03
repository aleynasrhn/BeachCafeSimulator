using UnityEngine;
using UnityEngine.UI;

public class ExtraButtonUI : MonoBehaviour
{
    [Header("Ekstra")]
    [SerializeField]
    private string extraName =
        "Ekstra Espresso";

    [SerializeField] private float price = 2f;


    [Header("Kilit")]
    [SerializeField] private int unlockDay = 1;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private Button button;


    private bool isUnlocked = true;


    public string ExtraName =>
        extraName;

    public float Price =>
        price;

    public int UnlockDay =>
        unlockDay;

    public bool IsLocked =>
        !isUnlocked;

    public bool IsUnlocked =>
        isUnlocked;


    // =========================================================
    // KİLİT DURUMU
    // =========================================================

    public void SetUnlocked(
        bool unlocked)
    {
        isUnlocked =
            unlocked;


        if (lockOverlay != null)
        {
            lockOverlay.SetActive(
                !unlocked
            );
        }


        if (button != null)
        {
            button.interactable =
                unlocked;
        }
    }
}