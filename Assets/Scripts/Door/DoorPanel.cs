using System.Collections;
using UnityEngine;

public class DoorPanel : MonoBehaviour
{
    [Header("Kapalı ve Açık Pozisyonlar (Local Position)")]
    [SerializeField] private Vector3 closedLocalPosition;
    [SerializeField] private Vector3 openLocalPosition;

    [Header("Hareket Hızı")]
    [SerializeField] private float moveSpeed = 2f;

    private Coroutine moveCoroutine;

    // =========================================================
    // KAPIYI AÇ
    // =========================================================

    public void MoveToOpen()
    {
        StartMove(openLocalPosition);
    }

    // =========================================================
    // KAPIYI KAPAT
    // =========================================================

    public void MoveToClosed()
    {
        StartMove(closedLocalPosition);
    }

    // =========================================================
    // HAREKETİ BAŞLAT
    // =========================================================

    private void StartMove(Vector3 target)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = StartCoroutine(MoveRoutine(target));
    }

    // =========================================================
    // HAREKET DÖNGÜSÜ
    // =========================================================

    private IEnumerator MoveRoutine(Vector3 target)
    {
        while (Vector3.Distance(transform.localPosition, target) > 0.001f)
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                target,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.localPosition = target;

        moveCoroutine = null;
    }

    // =========================================================
    // INSPECTOR'DA MEVCUT KONUMU "KAPALI" OLARAK KAYDET
    // (Sağ tık -> Copy Component değil, script başlığına sağ tık)
    // =========================================================

    [ContextMenu("Mevcut Konumu Kapalı Olarak Kaydet")]
    private void CaptureClosedPosition()
    {
        closedLocalPosition = transform.localPosition;
    }

    [ContextMenu("Mevcut Konumu Açık Olarak Kaydet")]
    private void CaptureOpenPosition()
    {
        openLocalPosition = transform.localPosition;
    }
}