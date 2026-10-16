using System.Collections;
using UnityEngine;

/// <summary>
/// Menteşe noktasındaki boş objeye (DoorPivot) eklenir.
/// Kapalı pozisyon = oyun başladığındaki rotasyon.
/// Açık pozisyon = kapalı pozisyondan Y ekseninde openAngle kadar dönmüş hali.
/// NPC her zaman Open()/Close() çağırır (kilide takılmaz).
/// Oyuncu sadece Toggle() ile açabilir/kapatabilir, kilitliyse hiçbir şey olmaz.
/// </summary>
public class ToiletDoor : MonoBehaviour
{
    [Header("Açılma")]
    [Tooltip("Kapalı pozisyondan Y ekseninde kaç derece dönecek. Yanlış tarafa açılıyorsa -90 yap.")]
    [SerializeField] private float openAngle = 90f;

    [Tooltip("Büyüdükçe kapı daha hızlı açılır/kapanır.")]
    [SerializeField] private float smoothSpeed = 6f;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Quaternion targetRotation;

    public bool IsOpen { get; private set; }

    public bool IsLocked { get; private set; }

    public bool IsMoving =>
        Quaternion.Angle(
            transform.localRotation,
            targetRotation
        ) > 0.01f;

    private void Awake()
    {
        closedRotation =
            transform.localRotation;

        openRotation =
            closedRotation *
            Quaternion.Euler(0f, openAngle, 0f);

        targetRotation =
            closedRotation;
    }

    private void Update()
    {
        if (!IsMoving)
            return;

        float t =
            1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);

        transform.localRotation =
            Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                t
            );

        if (Quaternion.Angle(
            transform.localRotation,
            targetRotation) < 0.3f)
        {
            transform.localRotation =
                targetRotation;
        }
    }

    // =========================================================
    // NPC BUNLARI ÇAĞIRIR (kilide bakmaz)
    // =========================================================

    public void Open()
    {
        IsOpen = true;
        targetRotation = openRotation;
    }

    public void Close()
    {
        IsOpen = false;
        targetRotation = closedRotation;
    }

    // =========================================================
    // SADECE OYUNCU İÇİN
    // =========================================================

    public void Toggle()
    {
        if (IsOpen)
            Close();
        else
            Open();
    }

    public void SetLocked(bool locked)
    {
        IsLocked = locked;
    }

    public IEnumerator WaitUntilStopped()
    {
        while (IsMoving)
            yield return null;
    }

    [ContextMenu("Test: Aç")]
    private void TestOpen() => Open();

    [ContextMenu("Test: Kapat")]
    private void TestClose() => Close();
}