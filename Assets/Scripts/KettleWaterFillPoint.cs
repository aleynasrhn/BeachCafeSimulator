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
    [SerializeField]
    private Vector3 positionOffset =
        Vector3.zero;

    [SerializeField]
    private Vector3 rotationOffsetEuler =
        Vector3.zero;

    [Header("Smooth Yerleşim")]
    [SerializeField] private float moveDuration = 0.35f;

    [Header("Su Dolumu")]
    [SerializeField] private float fillDuration = 3f;

    private PickupItem dockedKettle;
    private KettleWaterState kettleState;

    private bool isOccupied = false;

    private float fillTimer = 0f;

    private Coroutine moveCoroutine;


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
        // KETTLE ZATEN BURADAYSA → AL
        // =====================================================

        if (isOccupied)
        {
            if (dockedKettle != null)
            {
                dockedKettle.ForcePickUp(player);
            }

            dockedKettle = null;
            kettleState = null;

            isOccupied = false;

            fillTimer = 0f;

            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }

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


        // =====================================================
        // HEDEF POZİSYON
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

        fillTimer = 0f;


        // =====================================================
        // SMOOTH YERLEŞTİR
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
    // SMOOTH KETTLE HAREKETİ
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


        // =====================================================
        // TAM HEDEFE OTURT
        // =====================================================

        kettle.PlaceAtExact(
            targetPosition,
            targetRotation
        );


        // Oyuncunun elinden çıkar
        player.SetHeldItem(null);

        moveCoroutine = null;


        Debug.Log(
            "Kettle musluğun altındaki dolum noktasına yerleştirildi."
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
        // KAPAK KONTROLÜ
        // =====================================================

        KettleLidInteraction lid =
            dockedKettle.GetComponentInChildren<KettleLidInteraction>();

        if (lid == null)
        {
            fillTimer = 0f;
            return;
        }


        // =====================================================
        // KAPAK KAPALIYSA SU DOLMASIN
        // =====================================================

        if (!lid.IsOpen)
        {
            fillTimer = 0f;
            return;
        }


        // =====================================================
        // KAPAK AÇIK + MUSLUK AÇIK
        // =====================================================

        if (faucet != null &&
            faucet.IsOpen &&
            !kettleState.IsFull)
        {
            fillTimer += Time.deltaTime;

            if (fillTimer >= fillDuration)
            {
                kettleState.FillCompletely();

                fillTimer = 0f;

                Debug.Log(
                    "Kettle suyla tamamen dolduruldu."
                );
            }
        }
        else
        {
            fillTimer = 0f;
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