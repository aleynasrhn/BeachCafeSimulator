using UnityEngine;
using UnityEngine.AI;

public class NPCTestMovement : MonoBehaviour
{
    [SerializeField] private Transform target;

    private NavMeshAgent agent;
    private Animator animator;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent == null)
        {
            Debug.LogError("Lisa üzerinde NavMeshAgent bulunamadı!");
            return;
        }

        if (animator == null)
        {
            Debug.LogError("Lisa üzerinde Animator bulunamadı!");
            return;
        }

        if (target == null)
        {
            Debug.LogError("TestTarget atanmadı!");
            return;
        }

        agent.SetDestination(target.position);
    }

    private void Update()
    {
        if (agent == null || animator == null)
            return;

        float speed = agent.velocity.magnitude;

        animator.SetFloat("Speed", speed);
    }
}