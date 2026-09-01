using UnityEngine;

public class KettleWaterState : MonoBehaviour
{
    [Header("Su Kullanım Hakkı")]
    [SerializeField] private int maxWaterUses = 2;

    private int currentWaterUses = 0;

    // Su şu anda sıcak mı?
    private bool isHot = false;

    public int CurrentWaterUses => currentWaterUses;

    public int MaxWaterUses => maxWaterUses;

    public bool HasWater =>
        currentWaterUses > 0;

    public bool IsFull =>
        currentWaterUses >= maxWaterUses;

    public bool IsHot =>
        isHot;


    // =========================================================
    // SU DOLDUR
    // =========================================================

    public void FillCompletely()
    {
        currentWaterUses = maxWaterUses;

        // Yeni su doldurulduğu için henüz sıcak değil.
        isHot = false;

        Debug.Log(
            "Kettle suyla tamamen dolduruldu. Kullanım: " +
            currentWaterUses
        );
    }


    // =========================================================
    // SICAK SU YAP
    // =========================================================

    public void SetHot()
    {
        if (!HasWater)
        {
            Debug.Log(
                "Kettle'da su olmadığı için sıcak hale getirilemez."
            );

            return;
        }

        isHot = true;

        Debug.Log(
            "Kettle'daki su sıcak hale geldi."
        );
    }


    // =========================================================
    // SICAK SU KULLAN
    // =========================================================

    public bool ConsumeHotWater()
    {
        if (!HasWater)
            return false;

        if (!IsHot)
        {
            Debug.Log(
                "Kettle'daki su henüz sıcak değil."
            );

            return false;
        }

        currentWaterUses--;

        Debug.Log(
            "Sıcak su kullanıldı. Kalan kullanım: " +
            currentWaterUses
        );


        if (currentWaterUses <= 0)
        {
            currentWaterUses = 0;
            isHot = false;
        }

        return true;
    }


    // =========================================================
    // BOŞALT
    // =========================================================

    public void Empty()
    {
        currentWaterUses = 0;
        isHot = false;
    }
}