using System;
using System.IO;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public int currentDay = 1;
    public float money = 0f;
}

/// <summary>
/// Kayıt dosyasını Application.persistentDataPath altında
/// JSON olarak tutar. Unlock durumları ayrıca kaydedilmiyor —
/// DayManager.SetDay() zaten OnDayChanged eventini tetikleyip
/// UnlockManager'ın doğru günün kilitlerini otomatik uygulamasını
/// sağlıyor, bu yüzden sadece gün numarasını saklamak yeterli.
/// </summary>
public static class SaveManager
{
    private const string SaveFileName = "savegame.json";

    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, SaveFileName);

    // Ana menüden oyun sahnesine "bu yüklemede kayıtlı veriyi
    // uygula" bilgisini taşır. Sahne geçişinde sıfırlanmaz
    // (static), GameBootstrap okuyup kendi temizler.
    public static bool PendingLoadFromSave { get; set; } = false;

    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }

    public static void Save(GameSaveData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);

            Debug.Log($"Oyun kaydedildi: {SavePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Kayıt başarısız: {e.Message}");
        }
    }

    public static GameSaveData Load()
    {
        if (!HasSave())
        {
            Debug.LogWarning("Yüklenecek kayıt bulunamadı.");
            return null;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<GameSaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Kayıt yüklenemedi: {e.Message}");
            return null;
        }
    }

    public static void DeleteSave()
    {
        if (HasSave())
        {
            File.Delete(SavePath);
            Debug.Log("Kayıt silindi.");
        }
    }
}