using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    [Header("NPC Noktaları")]
    [SerializeField] private Transform cafeEntrancePoint;
    [SerializeField] private Transform queuePoint;

    private Transform assignedSeat;

    private NavMeshAgent agent;
    private Animator animator;

    private Order customerOrder;

    public Order CustomerOrder => customerOrder;

    private enum NPCState
    {
        GoingToEntrance,
        GoingToQueue,
        Waiting,
        GoingToTable,
        AtTable
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

        agent.SetDestination(
            cafeEntrancePoint.position
        );
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

                    agent.SetDestination(
                        queuePoint.position
                    );
                }

                break;


            case NPCState.GoingToQueue:

                if (HasReachedDestination())
                {
                    StopAtQueue();

                    CreateCustomerOrder();
                }

                break;


            case NPCState.Waiting:

                // Sipariş onaylanmasını bekliyor.

                break;


            case NPCState.GoingToTable:

                if (HasReachedDestination())
                {
                    ArriveAtTable();
                }

                break;


            case NPCState.AtTable:

                // Şimdilik burada bekleyecek.

                break;
        }

        UpdateAnimation();
    }

    private bool HasReachedDestination()
    {
        if (agent.pathPending)
            return false;

        if (!agent.hasPath)
            return false;

        return agent.remainingDistance <=
               agent.stoppingDistance;
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

        Debug.Log(
            $"{gameObject.name} QueuePoint'e ulaştı ve bekliyor."
        );
    }

    private void CreateCustomerOrder()
    {
        customerOrder = new Order();

        customerOrder.coffeeType =
            (CoffeeType)Random.Range(0, 4);

        customerOrder.size =
            (CupSize)Random.Range(0, 3);

        customerOrder.reward = 0;

        customerOrder.timeLimit = 90f;


        string[] paymentMethods =
        {
            "Nakit Ödeme",
            "Kart Ödeme"
        };

        customerOrder.preferredPaymentMethod =
            paymentMethods[
                Random.Range(
                    0,
                    paymentMethods.Length
                )
            ];


        if (customerOrder.coffeeType ==
            CoffeeType.Espresso)
        {
            customerOrder.espressoShot =
                Random.value > 0.5f
                    ? EspressoShotButtonUI.ShotType.Single
                    : EspressoShotButtonUI.ShotType.Double;
        }


        if (Random.value > 0.5f)
        {
            string[] extras =
            {
                "Ekstra Espresso",
                "Tarçın",
                "Çikolata Şurubu",
                "Karamel Şurubu",
                "Vanilya Şurubu"
            };

            customerOrder.requestedExtras.Add(
                extras[
                    Random.Range(
                        0,
                        extras.Length
                    )
                ]
            );
        }


        Debug.Log(
            $"{gameObject.name} sipariş oluşturdu: " +
            $"{customerOrder.coffeeType}"
        );


        if (OrderScreenUI.Instance != null)
        {
            OrderScreenUI.Instance.SetCustomerOrder(
                customerOrder
            );
        }
        else
        {
            Debug.LogWarning(
                "OrderScreenUI.Instance bulunamadı!"
            );
        }
    }

    // =========================================================
    // SİPARİŞ ONAYLANDI
    // =========================================================

    public void ConfirmCustomerOrder()
    {
        if (currentState != NPCState.Waiting)
        {
            Debug.LogWarning(
                $"{gameObject.name} şu anda sipariş beklemiyor."
            );

            return;
        }

        if (SeatManager.Instance == null)
        {
            Debug.LogError("SeatManager sahnede bulunamadı!");
            return;
        }

        assignedSeat = SeatManager.Instance.GetFreeSeat();

        if (assignedSeat == null)
        {
            Debug.LogWarning(
                $"{gameObject.name} için boş sandalye bulunamadı."
            );

            return;
        }

        currentState = NPCState.GoingToTable;

        agent.isStopped = false;

        agent.SetDestination(
            assignedSeat.position
        );

        Debug.Log(
            $"{gameObject.name} siparişi onaylandı. " +
            "Masaya gidiyor."
        );
    }

    private void ArriveAtTable()
    {
        currentState = NPCState.AtTable;

        agent.isStopped = true;
        agent.ResetPath();

        // NPC gidebildiği en yakın noktaya vardı.
        // Tam sandalyeye "ışınlayıp" oturma pozuna sabitliyoruz.
        agent.enabled = false;

        transform.position = assignedSeat.position;
        transform.rotation = assignedSeat.rotation;

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsSitting", true);
        }

        Debug.Log(
            $"{gameObject.name} masaya ulaştı."
        );
    }

    private void UpdateAnimation()
    {
        if (animator == null || agent == null)
            return;

        if (currentState == NPCState.Waiting ||
            currentState == NPCState.AtTable)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        animator.SetFloat(
            "Speed",
            agent.velocity.magnitude
        );
    }
}