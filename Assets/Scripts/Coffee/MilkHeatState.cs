/// <summary>
/// Sütün ısınma durumu.
/// Sıralama önemli: değer büyüdükçe süt daha çok ısınmış demektir.
/// </summary>
public enum MilkHeatState
{
    Cold,             // Hiç ısıtılmadı
    Underheated,      // 0 - 3 sn  (yetersiz)
    LatteReady,       // 3 - 4 sn  (Latte, mükemmel)
    LatteOkay,        // 4 - 5 sn  (Latte, kabul edilir ama kalite düşük)
    CappuccinoOkay,   // 5 - 6 sn  (Cappuccino, kabul edilir ama kalite düşük)
    CappuccinoReady,  // 6 - 7 sn  (Cappuccino, mükemmel)
    Burnt             // 7 - 10 sn (yanmış)
}


public static class MilkHeatRules
{
    // Süre ayarları (saniye). Oyunu dengelemek için buradan değiştir.
    public const float LatteMin = 3f;
    public const float LatteMax = 4f;        // mükemmel latte burada biter (yeşil ışık)
    public const float LatteOkayMax = 5f;    // 4-5 arası latte için kabul edilir
    public const float CappuccinoMin = 6f;   // mükemmel cappuccino burada başlar
    public const float BurntMin = 7f;
    public const float MaxSeconds = 10f;


    public static MilkHeatState GetState(
        float heatSeconds)
    {
        if (heatSeconds <= 0.01f)
            return MilkHeatState.Cold;

        if (heatSeconds < LatteMin)
            return MilkHeatState.Underheated;

        if (heatSeconds < LatteMax)
            return MilkHeatState.LatteReady;

        if (heatSeconds < LatteOkayMax)
            return MilkHeatState.LatteOkay;

        if (heatSeconds < CappuccinoMin)
            return MilkHeatState.CappuccinoOkay;

        if (heatSeconds < BurntMin)
            return MilkHeatState.CappuccinoReady;

        return MilkHeatState.Burnt;
    }


    // Latte için kabul edilen ısı mı?
    public static bool IsLatteHeat(
        MilkHeatState state)
    {
        return state == MilkHeatState.LatteReady ||
               state == MilkHeatState.LatteOkay;
    }


    // Cappuccino için kabul edilen ısı mı?
    public static bool IsCappuccinoHeat(
        MilkHeatState state)
    {
        return state == MilkHeatState.CappuccinoOkay ||
               state == MilkHeatState.CappuccinoReady;
    }


    // Tam isabet mi? (bahşiş ve puan için)
    public static bool IsPerfect(
        MilkHeatState state)
    {
        return state == MilkHeatState.LatteReady ||
               state == MilkHeatState.CappuccinoReady;
    }
}