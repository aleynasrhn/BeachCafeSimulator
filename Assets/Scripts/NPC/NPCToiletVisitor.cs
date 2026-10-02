using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NPCController))]
[RequireComponent(typeof(NavMeshAgent))]
public class NPCToiletVisitor : MonoBehaviour
{
    [Header("Gitme İhtimali")]
    [Tooltip("0.18 ≈ 5-6 müşteride 1")]
    [Range(0f, 1f)]
    [SerializeField] private float toiletVisitChance = 0.18f;

    [Header("Kabinde Kalma Süresi")]
    [SerializeField] private float minToiletTime = 10f;
    [SerializeField] private float maxToiletTime = 15f;

    [Header("Kapı Açma Animasyonu")]
    [Tooltip("Animator'daki Trigger parametresinin adı.")]
    [SerializeField] private string openDoorTrigger = "OpenDoor";

    [SerializeField] private float openDoorAnimDuration = 1.5f;

    [SerializeField] private float doorOpenDelay = 0.6f;

    private NPCController owner;
    private NavMeshAgent agent;
    private Animator animator;

    private float defaultMoveSpeed;

    private ToiletStall reservedStall;

    private void Awake()
    {
        owner = GetComponent<NPCController>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        defaultMoveSpeed = agent.speed;
    }

    // =========================================================
    // ÇAĞRILAN NOKTA: NPCController.StartLeavingWalk()
    // =========================================================

    public bool TryStartVisit()
    {
        if (!owner.WasOrderCorrect)
            return false;

        if (ToiletManager.Instance == null)
            return false;

        if (Random.value > toiletVisitChance)
            return false;

        // ---------------------------------------------
        // ODA KAPISI VE NOKTALARI DOĞRU ATANMIŞ MI?
        // ---------------------------------------------

        ToiletManager manager = ToiletManager.Instance;

        if (manager.RoomDoor == null)
        {
            Debug.LogError(
                "ToiletManager: Room Door alanı Inspector'da " +
                "atanmamış (None)! Tuvalet ziyareti iptal."
            );

            return false;
        }

        if (manager.RoomEntryPoint == null ||
            manager.RoomInsidePoint == null)
        {
            Debug.LogError(
                "ToiletManager: Room Entry Point ya da " +
                "Room Inside Point atanmamış! Tuvalet ziyareti iptal."
            );

            return false;
        }

        reservedStall =
            manager.TryReserveStall();

        if (reservedStall == null)
        {
            Debug.Log(
                $"{name}: Tuvalete gitmek istedi ama " +
                "boş/temiz kabin yok."
            );

            return false;
        }

        // ---------------------------------------------
        // KABİN KAPISI VE NOKTALARI DOĞRU ATANMIŞ MI?
        // ---------------------------------------------

        if (reservedStall.Door == null)
        {
            Debug.LogError(
                $"{reservedStall.name}: Door alanı Inspector'da " +
                "atanmamış (None)! Tuvalet ziyareti iptal."
            );

            reservedStall.Release();
            reservedStall = null;

            return false;
        }

        if (reservedStall.EntryPoint == null ||
            reservedStall.InsidePoint == null)
        {
            Debug.LogError(
                $"{reservedStall.name}: Entry Point ya da " +
                "Inside Point atanmamış! Tuvalet ziyareti iptal."
            );

            reservedStall.Release();
            reservedStall = null;

            return false;
        }

        owner.EnterToiletSequence();

        agent.isStopped = false;

        agent.SetDestination(
            manager.RoomEntryPoint.position
        );

        StartCoroutine(ToiletRoutine());

        Debug.Log(
            $"{name} tuvalete gidiyor → {reservedStall.name}"
        );

        return true;
    }

    private IEnumerator ToiletRoutine()
    {
        ToiletManager manager = ToiletManager.Instance;
        ToiletStall stall = reservedStall;

        // ---------------------------------------------
        // ODA KAPISINA YÜRÜ (NavMesh)
        // ---------------------------------------------

        yield return WaitForAgentArrival();

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            Debug.LogWarning(
                $"{name}: Tuvalete yol bulunamadı, iptal."
            );

            stall.Release();
            reservedStall = null;

            owner.FinishToiletSequence();

            yield break;
        }

        agent.isStopped = true;
        agent.ResetPath();

        yield return RotateTo(manager.RoomEntryPoint.rotation, 0.3f);

        // ---------------------------------------------
        // ODA KAPISINI AÇ
        // ---------------------------------------------

        yield return OpenDoorWithAnimation(manager.RoomDoor);

        agent.enabled = false;

        yield return WalkManually(manager.RoomInsidePoint.position);

        manager.RoomDoor.Close();

        // ---------------------------------------------
        // KABİN KAPISINA YÜRÜ (elle)
        // ---------------------------------------------

        yield return WalkManually(stall.EntryPoint.position);

        yield return RotateTo(stall.EntryPoint.rotation, 0.3f);

        // ---------------------------------------------
        // KABİN KAPISINI AÇ
        // ---------------------------------------------

        yield return OpenDoorWithAnimation(stall.Door);

        yield return WalkManually(stall.InsidePoint.position);

        yield return RotateTo(stall.InsidePoint.rotation, 0.3f);

        stall.Door.Close();

        yield return stall.Door.WaitUntilStopped();

        // ---------------------------------------------
        // KABİNDE BEKLE
        // ---------------------------------------------

        yield return new WaitForSeconds(
            Random.Range(minToiletTime, maxToiletTime)
        );

        // ---------------------------------------------
        // ÇIK (kapı AÇIK kalır, kabin kirlenir)
        // ---------------------------------------------

        stall.Door.Open();

        yield return stall.Door.WaitUntilStopped();

        yield return WalkManually(stall.EntryPoint.position);

        stall.FinishUse();

        reservedStall = null;

        // ---------------------------------------------
        // ODADAN ÇIK
        // ---------------------------------------------

        yield return WalkManually(manager.RoomInsidePoint.position);

        manager.RoomDoor.Open();

        yield return manager.RoomDoor.WaitUntilStopped();

        yield return WalkManually(manager.RoomEntryPoint.position);

        manager.RoomDoor.Close();

        // ---------------------------------------------
        // AGENT'I GERİ AÇ, ÇIKIŞA DÖN
        // ---------------------------------------------

        agent.enabled = true;

        agent.Warp(transform.position);

        owner.FinishToiletSequence();

        Debug.Log($"{name} tuvaletten çıktı.");
    }

    private IEnumerator WaitForAgentArrival()
    {
        while (agent.pathPending ||
               agent.remainingDistance > agent.stoppingDistance)
        {
            if (animator != null)
            {
                animator.SetFloat(
                    "Speed",
                    agent.velocity.magnitude
                );
            }

            if (!agent.pathPending &&
                agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                yield break;
            }

            yield return null;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    private IEnumerator WalkManually(Vector3 target)
    {
        while ((transform.position - target).sqrMagnitude > 0.0025f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                defaultMoveSpeed * Time.deltaTime
            );

            Vector3 flat = target - transform.position;
            flat.y = 0f;

            if (flat.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(flat);

                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    look,
                    360f * Time.deltaTime
                );
            }

            if (animator != null)
            {
                animator.SetFloat("Speed", defaultMoveSpeed);
            }

            yield return null;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    private IEnumerator RotateTo(Quaternion target, float duration)
    {
        Quaternion start = transform.rotation;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            transform.rotation = Quaternion.Slerp(
                start,
                target,
                Mathf.SmoothStep(0f, 1f, t)
            );

            yield return null;
        }

        transform.rotation = target;
    }

    private IEnumerator OpenDoorWithAnimation(ToiletDoor door)
    {
        // ---------------------------------------------
        // GÜVENLİK: door null gelirse (Inspector'da
        // atanmamışsa) burada net bir hata basıp çık,
        // sessizce crash olma.
        // ---------------------------------------------

        if (door == null)
        {
            Debug.LogError(
                $"{name}: OpenDoorWithAnimation'a null kapı " +
                "gönderildi! ToiletManager veya ToiletStall " +
                "Inspector alanlarını kontrol et."
            );

            yield break;
        }

        if (door.IsOpen)
            yield break;

        float startTime = Time.time;

        if (animator != null &&
            !string.IsNullOrEmpty(openDoorTrigger))
        {
            animator.ResetTrigger(openDoorTrigger);
            animator.SetTrigger(openDoorTrigger);
        }

        yield return new WaitForSeconds(doorOpenDelay);

        door.Open();

        yield return door.WaitUntilStopped();

        float remaining =
            openDoorAnimDuration - (Time.time - startTime);

        if (remaining > 0f)
        {
            yield return new WaitForSeconds(remaining);
        }
    }

    private void OnDestroy()
    {
        if (reservedStall != null)
        {
            reservedStall.Release();
        }
    }
}