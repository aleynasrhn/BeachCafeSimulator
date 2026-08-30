using UnityEngine;

public class KettleWaterState : MonoBehaviour
{
    [Header("Su Kullanım Hakkı")]
    [SerializeField] private int maxWaterUses = 2;

    private int currentWaterUses = 0;

    public int CurrentWaterUses => currentWaterUses;

    public int MaxWaterUses => maxWaterUses;

    public bool HasWater =>
        currentWaterUses > 0;

    public bool IsFull =>
        currentWaterUses >= maxWaterUses;


    public void FillCompletely()
    {
        currentWaterUses = maxWaterUses;

        Debug.Log(
            "Kettle suyla tamamen dolduruldu. Kullanım: " +
            currentWaterUses
        );
    }


    public bool ConsumeHotWater()
    {
        if (currentWaterUses <= 0)
            return false;

        currentWaterUses--;

        Debug.Log(
            "Sıcak su kullanıldı. Kalan kullanım: " +
            currentWaterUses
        );

        return true;
    }


    public void Empty()
    {
        currentWaterUses = 0;
    }
}