using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class KettleWaterFillPoint : MonoBehaviour, IInteractable
{
    [Header("Musluk")]
    [SerializeField] private FaucetInteraction faucet;

    [Header("Kettle")]
    [SerializeField] private string acceptedItemName = "Kettle";

    [Header("Yerleşim")]
    [SerializeField] private Vector3 positionOffset = Vector3.zero;
    [SerializeField] private Vector3 rotationOffsetEuler = Vector3.zero;

    [Header("Smooth Yerleşim")]
    [SerializeField] private float moveDuration = 0.35f;

    [Header("Su Dolumu")]
    [SerializeField] private float fillDuration = 3f;

    [Header("Sayaç")]
    [SerializeField] private KettleTimerDisplay timerDisplay;

    private PickupItem dockedKettle;
    private KettleWaterState kettleState;

    private bool isOccupied = false;
    private bool isFilling = false;

    private float fillTimer = 0f;

    private Coroutine moveCoroutine;

    private Collider kettleCollider;


    // =========================================================
    // PROMPT
    // =========================================================

    public string GetInteractPrompt()
    {
        return "";
    }


    // =========================================================
    // ETKİLEŞİM
    // =========================================================

    public void Interact(PlayerInteraction player)
    {
        if (player == null)
            return;


        // =====================================================
        // DOLUM SÜRÜYORSA KETTLE ALINAMAZ
        // =====================================================

        if (isOccupied && isFilling)
        {
            return;
        }


        // =====================================================
        // KETTLE ZATEN NOKTADAYSA → AL
        // =====================================================

        if (isOccupied)
        {
            if (dockedKettle != null)
            {
                if (kettleCollider != null)
                    kettleCollider.enabled = true;

                dockedKettle.ForcePickUp(player);
            }

            dockedKettle = null;
            kettleState = null;

            isOccupied = false;
            isFilling = false;

            fillTimer = 0f;

            if (timerDisplay != null)
                timerDisplay.HideTimer();

            return;
        }


        // =====================================================
        // ELDEKİ KETTLE
        // =====================================================

        PickupItem held =
            player.GetHeldItem();

        if (held == null)
            return;

        if (held.ItemName != acceptedItemName)
            return;


        KettleWaterState state =
            held.GetComponent<KettleWaterState>();

        if (state == null)
        {
            Debug.LogWarning(
                "KettleWaterFillPoint: KettleWaterState bulunamadı.",
                this
            );

            return;
        }


        // Kettle collider referansını al
        kettleCollider =
            held.GetComponent<Collider>();


        // =====================================================
        // HEDEF
        // =====================================================

        Vector3 targetPosition =
            transform.position +
            transform.TransformDirection(
                positionOffset
            );

        Quaternion targetRotation =
            transform.rotation *
            Quaternion.Euler(
                rotationOffsetEuler
            );


        // =====================================================
        // KETTLE'I KAYDET
        // =====================================================

        dockedKettle = held;
        kettleState = state;

        isOccupied = true;
        isFilling = false;
        fillTimer = 0f;


        if (timerDisplay != null)
        {
            timerDisplay.HideTimer();
        }


        // =====================================================
        // SMOOTH YERLEŞİM
        // =====================================================

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine =
            StartCoroutine(
                MoveKettleToPoint(
                    held,
                    targetPosition,
                    targetRotation,
                    player
                )
            );
    }


    // =========================================================
    // KETTLE'I SMOOTH ŞEKİLDE NOKTAYA TAŞI
    // =========================================================

    private IEnumerator MoveKettleToPoint(
        PickupItem kettle,
        Vector3 targetPosition,
        Quaternion targetRotation,
        PlayerInteraction player)
    {
        Vector3 startPosition =
            kettle.transform.position;

        Quaternion startRotation =
            kettle.transform.rotation;

        float elapsed = 0f;


        kettle.ClearHeldPositionOverride();


        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / moveDuration
                );

            float smoothT =
                t * t * (3f - 2f * t);


            kettle.transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    smoothT
                );


            kettle.transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    smoothT
                );


            yield return null;
        }


        // Tam noktaya oturt
        kettle.PlaceAtExact(
            targetPosition,
            targetRotation
        );


        // =====================================================
        // DOLUM BAŞLAMADAN ÖNCE KETTLE COLLIDERINI KAPAT
        // =====================================================

        if (kettleCollider != null)
        {
            kettleCollider.enabled = false;
        }


        player.SetHeldItem(null);

        moveCoroutine = null;


        Debug.Log(
            "Kettle musluk altına yerleştirildi."
        );
    }


    // =========================================================
    // SU DOLUMU
    // =========================================================

    private void Update()
    {
        if (!isOccupied)
            return;

        if (dockedKettle == null)
            return;

        if (kettleState == null)
            return;


        // =====================================================
        // KAPAK
        // =====================================================

        KettleLidInteraction lid =
            dockedKettle
            .GetComponentInChildren<KettleLidInteraction>();

        if (lid == null)
        {
            StopFilling();
            return;
        }


        // =====================================================
        // KAPAK KAPALIYSA DOLUM YOK
        // =====================================================

        if (!lid.IsOpen)
        {
            StopFilling();
            return;
        }


        // =====================================================
        // MUSLUK YOKSA
        // =====================================================

        if (faucet == null)
        {
            StopFilling();
            return;
        }


        // =====================================================
        // MUSLUK KAPALIYSA
        // =====================================================

        if (!faucet.IsOpen)
        {
            StopFilling();
            return;
        }


        // =====================================================
        // KETTLE ZATEN DOLUYSA
        // =====================================================

        if (kettleState.IsFull)
        {
            StopFilling();
            return;
        }


        // =====================================================
        // DOLUM BAŞLA
        // =====================================================

        if (!isFilling)
        {
            isFilling = true;
            fillTimer = 0f;

            if (timerDisplay != null)
            {
                timerDisplay.ShowTimer(
                    Mathf.CeilToInt(fillDuration)
                );
            }
        }


        // =====================================================
        // ZAMAN
        // =====================================================

        fillTimer += Time.deltaTime;


        int remainingSeconds =
            Mathf.CeilToInt(
                fillDuration - fillTimer
            );

        remainingSeconds =
            Mathf.Clamp(
                remainingSeconds,
                0,
                Mathf.CeilToInt(fillDuration)
            );


        if (timerDisplay != null &&
            remainingSeconds > 0)
        {
            timerDisplay.ShowTimer(
                remainingSeconds
            );
        }


        // =====================================================
        // DOLUM BİTTİ
        // =====================================================

        if (fillTimer >= fillDuration)
        {
            kettleState.FillCompletely();

            fillTimer = 0f;
            isFilling = false;


            if (timerDisplay != null)
            {
                timerDisplay.HideTimer();
            }


            // Artık tekrar alınabilir
            if (kettleCollider != null)
            {
                kettleCollider.enabled = true;
            }


            Debug.Log(
                "Kettle tamamen doldu. 2 kullanım hazır."
            );
        }
    }


    // =========================================================
    // DOLUMU DURDUR
    // =========================================================

    private void StopFilling()
    {
        if (!isFilling)
            return;

        isFilling = false;
        fillTimer = 0f;

        if (timerDisplay != null)
        {
            timerDisplay.HideTimer();
        }
    }


    // =========================================================
    // GİZMO
    // =========================================================

    private void OnDrawGizmos()
    {
        Gizmos.color =
            isOccupied
            ? Color.red
            : Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            0.06f
        );
    }
}