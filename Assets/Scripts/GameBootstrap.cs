using UnityEngine;

/// <summary>
/// Oyun sahnesi (BeachCafe) ilk açıldığında çalışır. Ana menüden
/// "Kaldığın Yerden Devam Et" ile gelindiyse kayıtlı günü ve parayı
/// yükler. "Yeni Oyun" ile gelindiyse hiçbir şey yapmaz, varsayılan
/// (Gün 1, 0$) değerlerle devam eder.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    private void Start()
    {
        if (!SaveManager.PendingLoadFromSave)
            return;

        SaveManager.PendingLoadFromSave = false;

        GameSaveData data = SaveManager.Load();

        if (data == null)
            return;

        if (DayManager.Instance != null)
        {
            DayManager.Instance.SetDay(data.currentDay);
        }

        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.SetMoney(data.money);
        }

        Debug.Log(
            $"Kayıt yüklendi: Gün {data.currentDay}, " +
            $"Para {data.money:0.00}$"
        );
    }
}