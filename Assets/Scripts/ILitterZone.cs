using UnityEngine;

/// <summary>
/// Hem AreaLitterZone (tuvalet kabinleri/ortak alan) hem de
/// CafeFloorLitterZone (kafe zemini) bu arayüzü uygular.
/// SweepableStain, temizlenince hangi yöneticiye haber vereceğini
/// bu sayede bilmesine gerek kalmadan bulur.
/// </summary>
public interface ILitterZone
{
    void NotifyLitterCleaned(GameObject obj);
}