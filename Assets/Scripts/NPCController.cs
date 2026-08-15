using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    [Header("NPC Noktaları")]
    [SerializeField] private Transform cafeEntrancePoint;
    [SerializeField] private Transform queuePoint;

    private NavMeshAgent agent;
    private Animator animator;

    private enum NPCState
    {
        GoingToEntrance,
        GoingToQueue,
        Waiting
    }

    private NPCState currentState;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (cafeEntrancePoint == null)
        {
            Debug.LogError("CafeEntrancePoint atanmadı!");
            return;
        }

        if (queuePoint == null)
        {
            Debug.LogError("QueuePoint atanmadı!");
            return;
        }

        agent.isStopped = false;

        currentState = NPCState.GoingToEntrance;
        agent.SetDestination(cafeEntrancePoint.position);
    }

    private void Update()
    {
        switch (currentState)
        {
            case NPCState.GoingToEntrance:

                if (HasReachedDestination())
                {
                    currentState = NPCState.GoingToQueue;

                    agent.isStopped = false;
                    agent.SetDestination(queuePoint.position);
                }

                break;

            case NPCState.GoingToQueue:

                if (HasReachedDestination())
                {
                    StopAtQueue();
                }

                break;

            case NPCState.Waiting:
                break;
        }

        UpdateAnimation();
    }

    /// <summary>
    /// SADECE mesafeye bakar - hız kontrolü YOK. NavMeshAgent hedefe yaklaşırken
    /// doğal olarak yavaşladığı için, hız da beklemek NPC'nin hedefi "geçip" birkaç
    /// adım daha atmasına sebep oluyordu.
    /// </summary>
    private bool HasReachedDestination()
    {
        if (agent.pathPending)
            return false;

        if (!agent.hasPath)
            return false;

        return agent.remainingDistance <= agent.stoppingDistance;
    }

    private void StopAtQueue()
    {
        currentState = NPCState.Waiting;

        agent.isStopped = true;
        agent.ResetPath();

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }

        Debug.Log($"{gameObject.name} QueuePoint'e ulaştı ve bekliyor.");
    }

    private void UpdateAnimation()
    {
        if (animator == null || agent == null)
            return;

        if (currentState == NPCState.Waiting)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        animator.SetFloat("Speed", agent.velocity.magnitude);
    }
}