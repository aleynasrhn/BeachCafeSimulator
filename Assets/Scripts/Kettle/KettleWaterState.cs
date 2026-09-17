using UnityEngine;

public class KettleWaterState : MonoBehaviour
{
    [Header("Su Kullanım Hakkı")]
    [SerializeField] private int maxWaterUses = 2;

    private int currentWaterUses = 0;
    private bool isHot = false;


    // =========================================================
    // GETTERS
    // =========================================================

    public int CurrentWaterUses =>
        currentWaterUses;

    public int MaxWaterUses =>
        maxWaterUses;

    public bool HasWater =>
        currentWaterUses > 0;

    public bool IsFull =>
        currentWaterUses >= maxWaterUses;

    public bool IsHot =>
        isHot;


    // =========================================================
    // KETTLE'I SU İLE DOLDUR
    // =========================================================

    public void FillCompletely()
    {
        // Kettle tamamen yeniden doldurulur.
        currentWaterUses = maxWaterUses;

        // Yeni su geldiği için eski sıcaklık silinir.
        isHot = false;

        Debug.Log(
            "Kettle tamamen su ile dolduruldu. " +
            "Kullanım: " +
            currentWaterUses +
            " | Sıcak: false"
        );
    }


    // =========================================================
    // SUYU SICAK YAP
    // =========================================================

    public void SetHot()
    {
        if (!HasWater)
        {
            Debug.Log(
                "Kettle'da su olmadığı için sıcak yapılamaz."
            );

            return;
        }

        isHot = true;

        Debug.Log(
            "Kettle'daki su sıcak hale geldi."
        );
    }


    // =========================================================
    // 1 SICAK SU KULLAN
    // =========================================================

    public bool ConsumeHotWater()
    {
        // Su yoksa
        if (!HasWater)
        {
            Debug.Log(
                "Kettle'da sıcak su kalmadı."
            );

            return false;
        }


        // Su sıcak değilse
        if (!IsHot)
        {
            Debug.Log(
                "Kettle'daki su henüz sıcak değil."
            );

            return false;
        }


        // 1 kullanım tüket
        currentWaterUses--;


        Debug.Log(
            "1 sıcak su kullanıldı. " +
            "Kalan kullanım: " +
            currentWaterUses
        );


        // Tamamen bittiyse
        if (currentWaterUses <= 0)
        {
            currentWaterUses = 0;

            // Artık sıcak su yok
            isHot = false;

            Debug.Log(
                "Kettle'daki tüm sıcak su bitti."
            );
        }


        return true;
    }


    // =========================================================
    // TAMAMEN BOŞALT
    // =========================================================

    public void Empty()
    {
        currentWaterUses = 0;
        isHot = false;

        Debug.Log(
            "Kettle tamamen boşaltıldı."
        );
    }
}