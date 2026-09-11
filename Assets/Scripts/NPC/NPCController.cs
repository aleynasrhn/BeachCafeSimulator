using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    // =========================================================
    // SAHNE NOKTALARI
    // =========================================================

    [Header("NPC Noktaları - Otomatik Bulunur")]
    [SerializeField] private Transform cafeEntrancePoint;

    [SerializeField] private Transform assignedQueuePoint;

    // =========================================================
    // ÇIKIŞ NOKTALARI
    // =========================================================

    [Header("Çıkış Noktaları - Otomatik Bulunur")]
    [SerializeField] private Transform exitPoint1;
    [SerializeField] private Transform exitPoint2;

    // =========================================================
    // SPAWN NOKTALARI
    // =========================================================

    [Header("Spawn Noktaları - Otomatik Bulunur")]
    [SerializeField] private Transform spawnPoint1;
    [SerializeField] private Transform spawnPoint2;

    private bool spawnedFromPoint1;

    // =========================================================
    // QUEUE SİSTEMİ
    // =========================================================

    private static readonly List<NPCController> queuedNPCs =
        new List<NPCController>();

    private static readonly List<Transform> queuePoints =
        new List<Transform>();

    private static bool queuePointsInitialized = false;

    // =========================================================
    // MASA
    // =========================================================

    private Transform assignedSeat;

    private DrinkPlacePoint assignedDrinkPlacePoint;

    // =========================================================
    // COMPONENTLER
    // =========================================================

    private NavMeshAgent agent;

    private Animator animator;

    // =========================================================
    // KARAKTER TÜRÜ
    // =========================================================

    [Header("Karakter")]
    [SerializeField] private bool isMale = false;

    // =========================================================
    // İÇME
    // =========================================================

    [Header("İçme")]
    [SerializeField] private NPCDrinkCupController drinkCupController;

    [SerializeField] private int totalDrinkCycles = 3;

    [SerializeField] private float minWaitBetweenDrinks = 6f;

    [SerializeField] private float maxWaitBetweenDrinks = 7f;

    private bool drinkSequenceActive = false;

    private int drinkCount = 0;

    private bool wasInDrinkState = false;

    private Coroutine waitBeforeNextDrinkCoroutine;

    // =========================================================
    // KAFEDEN AYRILMA
    // =========================================================

    [Header("Kafeden Ayrılma")]
    [SerializeField] private float minWaitBeforeLeaving = 4f;

    [SerializeField] private float maxWaitBeforeLeaving = 5f;

    private Coroutine waitBeforeLeaveCoroutine;

    // =========================================================
    // SİPARİŞ
    // =========================================================

    private Order customerOrder;

    public Order CustomerOrder =>
        customerOrder;

    // =========================================================
    // NPC DURUMLARI
    // =========================================================

    private enum NPCState
    {
        GoingToEntrance,
        GoingToQueue,
        Waiting,
        GoingToTable,
        AtTable,

        StandingUpFromTable,

        GoingToCafeEntranceExit,

        GoingToExit
    }

    private NPCState currentState;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        agent =
            GetComponent<NavMeshAgent>();

        animator =
            GetComponent<Animator>();

        // ---------------------------------------------------------
        // KADIN / ERKEK ANİMASYONU
        // ---------------------------------------------------------

        if (animator != null)
        {
            animator.SetBool(
                "IsMale",
                isMale
            );
        }

        // ---------------------------------------------------------
        // DRINK CONTROLLER
        // ---------------------------------------------------------

        if (drinkCupController == null)
        {
            drinkCupController =
                GetComponent<NPCDrinkCupController>();
        }

        if (drinkCupController == null)
        {
            Debug.LogError(
                $"{gameObject.name}: NPCDrinkCupController bulunamadı!"
            );
        }
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        FindScenePoints();

        if (!ValidateScenePoints())
            return;

        DetermineSpawnSide();

        if (animator != null)
        {
            animator.SetBool(
                "IsMale",
                isMale
            );
        }

        agent.isStopped = false;

        currentState =
            NPCState.GoingToEntrance;

        agent.SetDestination(
            cafeEntrancePoint.position
        );

        Debug.Log(
            $"{gameObject.name} CafeEntrancePoint'e gidiyor."
        );
    }

    // =========================================================
    // SAHNE NOKTALARINI BUL
    // =========================================================

    private void FindScenePoints()
    {
        // ---------------------------------------------------------
        // CAFE ENTRANCE
        // ---------------------------------------------------------

        if (cafeEntrancePoint == null)
        {
            cafeEntrancePoint =
                FindTransform(
                    "CafeEntrancePoint"
                );
        }

        // ---------------------------------------------------------
        // EXITLER
        // ---------------------------------------------------------

        if (exitPoint1 == null)
        {
            exitPoint1 =
                FindTransform(
                    "ExitPoint"
                );
        }

        if (exitPoint2 == null)
        {
            exitPoint2 =
                FindTransform(
                    "ExitPoint2"
                );
        }

        // ---------------------------------------------------------
        // SPAWN
        // ---------------------------------------------------------

        if (spawnPoint1 == null)
        {
            spawnPoint1 =
                FindTransform(
                    "SpawnPoint_1",
                    "SpawnPoint"
                );
        }

        if (spawnPoint2 == null)
        {
            spawnPoint2 =
                FindTransform(
                    "SpawnPoint_2",
                    "SpawnPoint2"
                );
        }

        // ---------------------------------------------------------
        // QUEUE NOKTALARI
        // ---------------------------------------------------------

        if (!queuePointsInitialized)
        {
            queuePoints.Clear();

            AddQueuePoint(
                "QueuePoint_1",
                "QueuePoint"
            );

            AddQueuePoint(
                "QueuePoint_2",
                "QueuePoint2"
            );

            AddQueuePoint(
                "QueuePoint_3",
                "QueuePoint3"
            );

            AddQueuePoint(
                "QueuePoint_4",
                "QueuePoint4"
            );

            queuePointsInitialized = true;
        }
    }

    // =========================================================
    // TRANSFORM BUL
    // =========================================================

    private Transform FindTransform(
        params string[] names)
    {
        foreach (string objectName in names)
        {
            GameObject obj =
                GameObject.Find(objectName);

            if (obj != null)
                return obj.transform;
        }

        return null;
    }

    // =========================================================
    // QUEUE NOKTASI EKLE
    // =========================================================

    private void AddQueuePoint(
        params string[] names)
    {
        Transform point =
            FindTransform(names);

        if (point == null)
            return;

        if (!queuePoints.Contains(point))
        {
            queuePoints.Add(point);
        }
    }

    // =========================================================
    // SPAWN TARAFINI BELİRLE
    // =========================================================

    private void DetermineSpawnSide()
    {
        if (spawnPoint1 == null &&
            spawnPoint2 == null)
        {
            spawnedFromPoint1 = true;
            return;
        }

        if (spawnPoint1 == null)
        {
            spawnedFromPoint1 = false;
            return;
        }

        if (spawnPoint2 == null)
        {
            spawnedFromPoint1 = true;
            return;
        }

        float distance1 =
            Vector3.Distance(
                transform.position,
                spawnPoint1.position
            );

        float distance2 =
            Vector3.Distance(
                transform.position,
                spawnPoint2.position
            );

        spawnedFromPoint1 =
            distance1 <= distance2;
    }

    // =========================================================
    // NOKTA KONTROLÜ
    // =========================================================

    private bool ValidateScenePoints()
    {
        bool valid = true;

        if (cafeEntrancePoint == null)
        {
            Debug.LogError(
                $"{gameObject.name}: " +
                "CafeEntrancePoint bulunamadı!"
            );

            valid = false;
        }

        if (exitPoint1 == null)
        {
            Debug.LogError(
                $"{gameObject.name}: " +
                "ExitPoint bulunamadı!"
            );

            valid = false;
        }

        if (exitPoint2 == null)
        {
            Debug.LogError(
                $"{gameObject.name}: " +
                "ExitPoint2 bulunamadı!"
            );

            valid = false;
        }

        if (queuePoints.Count == 0)
        {
            Debug.LogError(
                $"{gameObject.name}: " +
                "Hiç QueuePoint bulunamadı!"
            );

            valid = false;
        }

        return valid;
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        switch (currentState)
        {
            // =================================================
            // CAFE ENTRANCE
            // =================================================

            case NPCState.GoingToEntrance:

                if (HasReachedDestination())
                {
                    TryJoinQueue();
                }

                break;

            // =================================================
            // QUEUE
            // =================================================

            case NPCState.GoingToQueue:

                if (HasReachedDestination())
                {
                    StopAtQueue();
                }

                break;

            // =================================================
            // SİPARİŞ BEKLİYOR
            // =================================================

            case NPCState.Waiting:

                break;

            // =================================================
            // MASAYA GİDİYOR
            // =================================================

            case NPCState.GoingToTable:

                if (HasReachedDestination())
                {
                    ArriveAtTable();
                }

                break;

            // =================================================
            // MASADA
            // =================================================

            case NPCState.AtTable:

                break;

            // =================================================
            // KALKMA ANİMASYONU
            // =================================================

            case NPCState.StandingUpFromTable:

                break;

            // =================================================
            // CAFE ENTRANCE'A ÇIKIYOR
            // =================================================

            case NPCState.GoingToCafeEntranceExit:

                if (HasReachedDestination())
                {
                    GoToRandomExitPoint();
                }

                break;

            // =================================================
            // ÇIKIŞ
            // =================================================

            case NPCState.GoingToExit:

                if (HasReachedDestination())
                {
                    Debug.Log(
                        $"{gameObject.name} kafeden çıktı."
                    );

                    Destroy(gameObject);
                }

                break;
        }

        UpdateAnimation();

        HandleDrinkSequence();
    }

    // =========================================================
    // QUEUE'YA GİR
    // =========================================================

    private void TryJoinQueue()
    {
        // Zaten kuyruktaysa tekrar ekleme
        if (queuedNPCs.Contains(this))
            return;

        // ---------------------------------------------------------
        // KUYRUK DOLU
        // ---------------------------------------------------------

        if (queuedNPCs.Count >= queuePoints.Count)
        {
            Debug.Log(
                $"{gameObject.name}: " +
                "Kuyruk tamamen dolu!"
            );

            GoToOppositeExit();

            return;
        }

        // ---------------------------------------------------------
        // KUYRUĞA EKLE
        // ---------------------------------------------------------

        queuedNPCs.Add(
            this
        );

        int queueIndex =
            queuedNPCs.Count - 1;

        assignedQueuePoint =
            queuePoints[queueIndex];

        currentState =
            NPCState.GoingToQueue;

        agent.isStopped = false;

        agent.SetDestination(
            assignedQueuePoint.position
        );

        Debug.Log(
            $"{gameObject.name} → " +
            $"{assignedQueuePoint.name}"
        );
    }

    // =========================================================
    // KUYRUĞUN EN ÖNÜNDE Mİ?
    // =========================================================

    private bool IsFrontOfQueue()
    {
        return queuedNPCs.Count > 0 &&
               queuedNPCs[0] == this;
    }

    // =========================================================
    // QUEUE'DAN ÇIK
    // =========================================================

    private void RemoveFromQueue()
    {
        int index =
            queuedNPCs.IndexOf(
                this
            );

        if (index < 0)
            return;

        queuedNPCs.RemoveAt(
            index
        );

        assignedQueuePoint = null;

        // ---------------------------------------------------------
        // ARKADAKİLERİ ÖNE KAYDIR
        // ---------------------------------------------------------

        for (
            int i = index;
            i < queuedNPCs.Count;
            i++
        )
        {
            NPCController npc =
                queuedNPCs[i];

            if (npc == null)
                continue;

            npc.MoveForwardInQueue(
                queuePoints[i]
            );
        }

        Debug.Log(
            $"{gameObject.name} kuyruktan çıktı."
        );
    }

    // =========================================================
    // SIRADAKİ NPC ÖNE GELSİN
    // =========================================================

    private void MoveForwardInQueue(
        Transform newQueuePoint)
    {
        if (newQueuePoint == null)
            return;

        assignedQueuePoint =
            newQueuePoint;

        currentState =
            NPCState.GoingToQueue;

        if (agent == null ||
            !agent.enabled)
        {
            return;
        }

        agent.isStopped = false;

        agent.SetDestination(
            newQueuePoint.position
        );

        Debug.Log(
            $"{gameObject.name} öne geçti → " +
            $"{newQueuePoint.name}"
        );
    }

    // =========================================================
    // QUEUE NOKTASINA ULAŞTI
    // =========================================================

    private void StopAtQueue()
    {
        currentState =
            NPCState.Waiting;

        agent.isStopped = true;

        agent.ResetPath();

        if (animator != null)
        {
            animator.SetFloat(
                "Speed",
                0f
            );
        }

        // ---------------------------------------------------------
        // BU NPC'NİN SİPARİŞİ DAHA ÖNCE OLUŞTURULMADIYSA OLUŞTUR
        // ---------------------------------------------------------

        if (customerOrder == null)
        {
            CreateCustomerOrder();
        }

        // ---------------------------------------------------------
        // SADECE KUYRUĞUN EN ÖNÜNDEKİ NPC
        // SİPARİŞİNİ KASAYA GÖSTERİR
        // ---------------------------------------------------------

        if (IsFrontOfQueue())
        {
            if (OrderScreenUI.Instance != null)
            {
                OrderScreenUI.Instance.SetCustomerOrder(
                    customerOrder,
                    this
                );
            }
            else
            {
                Debug.LogWarning(
                    "OrderScreenUI.Instance bulunamadı!"
                );
            }
        }

        Debug.Log(
            $"{gameObject.name} " +
            $"{assignedQueuePoint?.name} " +
            "noktasında bekliyor."
        );
    }

    // =========================================================
    // KUYRUK DOLUYSA KARŞI ÇIKIŞ
    // =========================================================

    private void GoToOppositeExit()
    {
        Transform selectedExit;

        if (spawnedFromPoint1)
        {
            // SpawnPoint_1 → ExitPoint2
            selectedExit =
                exitPoint2;
        }
        else
        {
            // SpawnPoint_2 → ExitPoint
            selectedExit =
                exitPoint1;
        }

        if (selectedExit == null)
        {
            Debug.LogError(
                $"{gameObject.name}: " +
                "Karşı çıkış bulunamadı!"
            );

            return;
        }

        currentState =
            NPCState.GoingToExit;

        agent.isStopped = false;

        agent.SetDestination(
            selectedExit.position
        );

        Debug.Log(
            $"{gameObject.name}: " +
            "Kuyruk dolu olduğu için " +
            $"{selectedExit.name} çıkışına gidiyor."
        );
    }

    // =========================================================
    // HEDEFE ULAŞTI MI?
    // =========================================================

    private bool HasReachedDestination()
    {
        if (agent == null ||
            !agent.enabled)
        {
            return false;
        }

        if (agent.pathPending)
            return false;

        if (!agent.hasPath)
            return false;

        return agent.remainingDistance <=
               agent.stoppingDistance;
    }

    // =========================================================
    // MÜŞTERİ SİPARİŞİ
    // =========================================================

    private void CreateCustomerOrder()
    {
        customerOrder =
            new Order();

        // =====================================================
        // AÇIK KAHVELER
        // =====================================================

        List<CoffeeType> unlockedCoffees =
            GetUnlockedCoffeeTypes();

        if (unlockedCoffees.Count == 0)
        {
            Debug.LogError(
                "NPC siparişi oluşturulamadı: " +
                "Hiç açık kahve yok!"
            );

            return;
        }

        // =====================================================
        // RASTGELE KAHVE
        // =====================================================

        customerOrder.coffeeType =
            unlockedCoffees[
                Random.Range(
                    0,
                    unlockedCoffees.Count
                )
            ];

        // =====================================================
        // BOYUT
        // =====================================================

        if (customerOrder.coffeeType ==
            CoffeeType.Espresso)
        {
            customerOrder.size =
                CupSize.Small;
        }
        else
        {
            customerOrder.size =
                (CupSize)Random.Range(
                    0,
                    3
                );
        }

        // =====================================================
        // ÖDÜL
        // =====================================================

        customerOrder.reward =
            0;

        // =====================================================
        // SİPARİŞ SÜRESİ
        // =====================================================

        customerOrder.timeLimit =
            90f;

        // =====================================================
        // ÖDEME YÖNTEMİ
        // =====================================================

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

        // =====================================================
        // ESPRESSO SHOT
        // =====================================================

        if (customerOrder.coffeeType ==
            CoffeeType.Espresso)
        {
            customerOrder.espressoShot =
                Random.value > 0.5f
                    ? EspressoShotButtonUI.ShotType.Single
                    : EspressoShotButtonUI.ShotType.Double;
        }
        else
        {
            customerOrder.espressoShot =
                EspressoShotButtonUI.ShotType.Single;
        }

        // =====================================================
        // EKSTRA
        // =====================================================

        CreateRandomExtra();

        // =====================================================
        // EKSTRA ESPRESSO
        // =====================================================

        if (customerOrder.coffeeType !=
            CoffeeType.Espresso)
        {
            if (customerOrder.requestedExtras.Contains(
                "Ekstra Espresso"))
            {
                customerOrder.espressoShot =
                    EspressoShotButtonUI.ShotType.Double;
            }
        }

        // =====================================================
        // DEBUG
        // =====================================================

        string extraText =
            customerOrder.requestedExtras.Count > 0
                ? string.Join(
                    ", ",
                    customerOrder.requestedExtras
                )
                : "Yok";

        Debug.Log(
            $"{gameObject.name} sipariş oluşturdu: " +
            $"{customerOrder.coffeeType} | " +
            $"{customerOrder.size} | " +
            $"{customerOrder.espressoShot} | " +
            $"Extra: {extraText}"
        );

        // ---------------------------------------------------------
        // DİKKAT:
        // BURADA ORDER SCREEN'E GÖNDERME YOK.
        //
        // Sadece StopAtQueue() içinde,
        // kuyrukta EN ÖNDE olan NPC'nin siparişi
        // OrderScreenUI'ya gönderiliyor.
        // ---------------------------------------------------------
    }

    // =========================================================
    // AÇIK KAHVELER
    // =========================================================

    private List<CoffeeType> GetUnlockedCoffeeTypes()
    {
        List<CoffeeType> result =
            new List<CoffeeType>();

        if (!UnlockManager.InstanceExists)
        {
            result.Add(
                CoffeeType.Americano
            );

            result.Add(
                CoffeeType.Espresso
            );

            result.Add(
                CoffeeType.Latte
            );

            return result;
        }

        if (UnlockManager.Instance.IsCoffeeUnlocked(
            CoffeeType.Americano))
        {
            result.Add(
                CoffeeType.Americano
            );
        }

        if (UnlockManager.Instance.IsCoffeeUnlocked(
            CoffeeType.Espresso))
        {
            result.Add(
                CoffeeType.Espresso
            );
        }

        if (UnlockManager.Instance.IsCoffeeUnlocked(
            CoffeeType.Latte))
        {
            result.Add(
                CoffeeType.Latte
            );
        }

        if (UnlockManager.Instance.IsCoffeeUnlocked(
            CoffeeType.Cappuccino))
        {
            result.Add(
                CoffeeType.Cappuccino
            );
        }

        return result;
    }

    // =========================================================
    // RASTGELE EKSTRA
    // =========================================================

    private void CreateRandomExtra()
    {
        if (Random.value <= 0.5f)
            return;

        string[] allExtras =
        {
            "Ekstra Espresso",
            "Tarçın",
            "Çikolata Şurubu",
            "Karamel Şurubu",
            "Vanilya Şurubu"
        };

        List<string> unlockedExtras =
            new List<string>();

        foreach (string extra in allExtras)
        {
            if (customerOrder.coffeeType ==
                CoffeeType.Espresso &&
                extra == "Ekstra Espresso")
            {
                continue;
            }

            if (UnlockManager.InstanceExists &&
                UnlockManager.Instance.IsExtraUnlocked(
                    extra))
            {
                unlockedExtras.Add(
                    extra
                );
            }
        }

        if (unlockedExtras.Count == 0)
            return;

        string selectedExtra =
            unlockedExtras[
                Random.Range(
                    0,
                    unlockedExtras.Count
                )
            ];

        customerOrder.requestedExtras.Add(
            selectedExtra
        );
    }

    // =========================================================
    // SİPARİŞ ONAYLANDI
    // =========================================================

    public void ConfirmCustomerOrder()
    {
        if (currentState !=
            NPCState.Waiting)
        {
            Debug.LogWarning(
                $"{gameObject.name} şu anda sipariş beklemiyor."
            );

            return;
        }

        if (SeatManager.Instance == null)
        {
            Debug.LogError(
                "SeatManager sahnede bulunamadı!"
            );

            return;
        }

        // =====================================================
        // BOŞ SANDALYE
        // =====================================================

        assignedSeat =
            SeatManager.Instance.GetFreeSeat();

        if (assignedSeat == null)
        {
            Debug.LogWarning(
                $"{gameObject.name} için boş sandalye bulunamadı."
            );

            return;
        }

        // =====================================================
        // DRINK PLACE POINT
        // =====================================================

        assignedDrinkPlacePoint =
            SeatManager.Instance.GetDrinkPlacePoint(
                assignedSeat
            );

        if (assignedDrinkPlacePoint == null)
        {
            Debug.LogError(
                $"{gameObject.name} için " +
                $"{assignedSeat.name} koltuğuna karşılık gelen " +
                "DrinkPlacePoint bulunamadı!"
            );

            SeatManager.Instance.ReleaseSeat(
                assignedSeat
            );

            assignedSeat = null;

            return;
        }

        // =====================================================
        // KUYRUKTAN ÇIK
        // =====================================================

        RemoveFromQueue();

        assignedDrinkPlacePoint.SetCustomer(
            this
        );

        Debug.Log(
            $"{gameObject.name} bağlantısı kuruldu: " +
            $"{assignedSeat.name} → " +
            $"{assignedDrinkPlacePoint.gameObject.name}"
        );

        // =====================================================
        // MASAYA GİT
        // =====================================================

        currentState =
            NPCState.GoingToTable;

        agent.isStopped = false;

        agent.SetDestination(
            assignedSeat.position
        );

        Debug.Log(
            $"{gameObject.name} siparişi onaylandı. " +
            "Masaya gidiyor."
        );
    }

    // =========================================================
    // MASAYA ULAŞTI
    // =========================================================

    private void ArriveAtTable()
    {
        currentState =
            NPCState.AtTable;

        agent.isStopped = true;

        agent.ResetPath();

        agent.enabled = false;

        transform.position =
            assignedSeat.position;

        transform.rotation =
            assignedSeat.rotation;

        if (animator != null)
        {
            animator.SetFloat(
                "Speed",
                0f
            );

            animator.SetBool(
                "IsSitting",
                true
            );
        }

        Debug.Log(
            $"{gameObject.name} masaya ulaştı."
        );
    }

    // =========================================================
    // İÇME BAŞLAT
    // =========================================================

    public void StartDrinkSequence(
        PickupItem cup)
    {
        if (currentState !=
            NPCState.AtTable)
        {
            return;
        }

        if (drinkSequenceActive)
            return;

        if (animator == null)
            return;

        if (cup == null)
        {
            Debug.LogWarning(
                $"{gameObject.name} için içilecek kahve bulunamadı."
            );

            return;
        }

        if (drinkCupController == null)
        {
            Debug.LogWarning(
                $"{gameObject.name} için " +
                "NPCDrinkCupController bulunamadı."
            );

            return;
        }

        drinkCupController.SetCup(
            cup
        );

        drinkSequenceActive =
            true;

        drinkCount =
            0;

        wasInDrinkState =
            false;

        if (waitBeforeNextDrinkCoroutine != null)
        {
            StopCoroutine(
                waitBeforeNextDrinkCoroutine
            );

            waitBeforeNextDrinkCoroutine =
                null;
        }

        animator.ResetTrigger(
            "Drink"
        );

        animator.SetTrigger(
            "Drink"
        );

        Debug.Log(
            $"{gameObject.name} doğru kahveyi aldı. " +
            "İçme animasyonu başlatılıyor."
        );
    }

    // =========================================================
    // İÇME DÖNGÜSÜ
    // =========================================================

    private void HandleDrinkSequence()
    {
        if (!drinkSequenceActive ||
            animator == null)
        {
            return;
        }

        AnimatorStateInfo stateInfo =
            animator.GetCurrentAnimatorStateInfo(0);

        bool inDrinkState =
            stateInfo.IsName(
                "Drinking"
            );

        if (inDrinkState &&
            !wasInDrinkState)
        {
            drinkCount++;

            Debug.Log(
                $"{gameObject.name} içme animasyonu: " +
                $"{drinkCount}/{totalDrinkCycles}"
            );
        }

        if (!inDrinkState &&
            wasInDrinkState)
        {
            if (drinkCount < totalDrinkCycles)
            {
                if (waitBeforeNextDrinkCoroutine != null)
                {
                    StopCoroutine(
                        waitBeforeNextDrinkCoroutine
                    );
                }

                waitBeforeNextDrinkCoroutine =
                    StartCoroutine(
                        WaitThenDrinkAgain()
                    );
            }
            else
            {
                drinkSequenceActive =
                    false;

                Debug.Log(
                    $"{gameObject.name} tüm içme döngülerini " +
                    "tamamladı. Biraz sonra kalkıp kafeden çıkacak."
                );

                if (waitBeforeLeaveCoroutine != null)
                {
                    StopCoroutine(
                        waitBeforeLeaveCoroutine
                    );
                }

                waitBeforeLeaveCoroutine =
                    StartCoroutine(
                        WaitThenLeaveTable()
                    );
            }
        }

        wasInDrinkState =
            inDrinkState;
    }

    // =========================================================
    // TEKRAR İÇ
    // =========================================================

    private IEnumerator WaitThenDrinkAgain()
    {
        float waitTime =
            Random.Range(
                minWaitBetweenDrinks,
                maxWaitBetweenDrinks
            );

        yield return new WaitForSeconds(
            waitTime
        );

        if (animator == null)
            yield break;

        animator.ResetTrigger(
            "Drink"
        );

        animator.SetTrigger(
            "Drink"
        );

        waitBeforeNextDrinkCoroutine =
            null;
    }

    // =========================================================
    // MASADAN KALKMADAN ÖNCE
    // =========================================================

    private IEnumerator WaitThenLeaveTable()
    {
        float waitTime =
            Random.Range(
                minWaitBeforeLeaving,
                maxWaitBeforeLeaving
            );

        yield return new WaitForSeconds(
            waitTime
        );

        LeaveTable();

        waitBeforeLeaveCoroutine =
            null;
    }

    // =========================================================
    // MASADAN KALK
    // =========================================================

    private void LeaveTable()
    {
        if (cafeEntrancePoint == null)
        {
            Debug.LogError(
                $"{gameObject.name}: " +
                "CafeEntrancePoint bulunamadı!"
            );

            return;
        }

        currentState =
            NPCState.StandingUpFromTable;

        if (animator != null)
        {
            animator.SetBool(
                "IsSitting",
                false
            );
        }

        if (SeatManager.Instance != null &&
            assignedSeat != null)
        {
            SeatManager.Instance.ReleaseSeat(
                assignedSeat
            );
        }

        if (assignedDrinkPlacePoint != null)
        {
            assignedDrinkPlacePoint.SetCustomer(
                null
            );
        }

        Debug.Log(
            $"{gameObject.name} masadan kalkıyor."
        );
    }

    // =========================================================
    // KALKMA ANİMASYONU BİTTİ
    // =========================================================

    public void StartLeavingWalk()
    {
        if (currentState !=
            NPCState.StandingUpFromTable)
        {
            return;
        }

        if (agent == null)
        {
            Debug.LogError(
                $"{gameObject.name}: " +
                "NavMeshAgent bulunamadı!"
            );

            return;
        }

        if (cafeEntrancePoint == null)
        {
            Debug.LogError(
                $"{gameObject.name}: " +
                "CafeEntrancePoint bulunamadı!"
            );

            return;
        }

        agent.enabled = true;

        agent.Warp(
            transform.position
        );

        agent.isStopped = false;

        currentState =
            NPCState.GoingToCafeEntranceExit;

        agent.SetDestination(
            cafeEntrancePoint.position
        );

        Debug.Log(
            $"{gameObject.name} kalkma animasyonunu tamamladı. " +
            "CafeEntrancePoint'e yürüyor."
        );
    }

    // =========================================================
    // NORMAL ÇIKIŞ
    // =========================================================

    private void GoToRandomExitPoint()
    {
        if (exitPoint1 == null ||
            exitPoint2 == null)
        {
            Debug.LogError(
                $"{gameObject.name}: " +
                "Exit noktaları bulunamadı!"
            );

            return;
        }

        Transform selectedExit;

        if (Random.value < 0.5f)
        {
            selectedExit =
                exitPoint1;
        }
        else
        {
            selectedExit =
                exitPoint2;
        }

        currentState =
            NPCState.GoingToExit;

        agent.SetDestination(
            selectedExit.position
        );

        Debug.Log(
            $"{gameObject.name} çıkışa gidiyor → " +
            $"{selectedExit.name}"
        );
    }

    // =========================================================
    // ANİMASYON
    // =========================================================

    private void UpdateAnimation()
    {
        if (animator == null ||
            agent == null)
        {
            return;
        }

        if (currentState ==
            NPCState.Waiting ||
            currentState ==
            NPCState.AtTable ||
            currentState ==
            NPCState.StandingUpFromTable)
        {
            animator.SetFloat(
                "Speed",
                0f
            );

            return;
        }

        animator.SetFloat(
            "Speed",
            agent.enabled
                ? agent.velocity.magnitude
                : 0f
        );
    }

    // =========================================================
    // TEMİZLİK
    // =========================================================

    private void OnDestroy()
    {
        int index =
            queuedNPCs.IndexOf(
                this
            );

        if (index >= 0)
        {
            queuedNPCs.RemoveAt(
                index
            );

            for (
                int i = index;
                i < queuedNPCs.Count;
                i++
            )
            {
                if (queuedNPCs[i] != null &&
                    i < queuePoints.Count)
                {
                    queuedNPCs[i]
                        .MoveForwardInQueue(
                            queuePoints[i]
                        );
                }
            }
        }

        if (queuedNPCs.Count == 0)
        {
            queuePoints.Clear();
            queuePointsInitialized = false;
        }
    }
}