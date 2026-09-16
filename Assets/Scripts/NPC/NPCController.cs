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
    // SPAWN GÖREVİ
    // =========================================================

    [Header("Spawn Görevi")]
    [SerializeField] private bool isCafeCustomer = true;

    private NPCSpawner npcSpawner;

    private bool cafeCustomerRegistered = false;

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
    // SİPARİŞ BEKLEME (MASADA) - KAHVE TÜRÜNE GÖRE
    // =========================================================

    [System.Serializable]
    public class CoffeeWaitTime
    {
        public CoffeeType coffeeType;

        public float minWaitTime = 60f;

        public float maxWaitTime = 70f;
    }

    [Header("Sipariş Bekleme - Kahve Türüne Göre Süreler")]
    [SerializeField]
    private List<CoffeeWaitTime> coffeeWaitTimes =
        new List<CoffeeWaitTime>();

    [Header("Sipariş Bekleme - Varsayılan Süre")]
    [Tooltip("Listede kahve türü için özel süre tanımlanmamışsa bu kullanılır.")]
    [SerializeField] private float defaultMinOrderWaitTime = 60f;

    [SerializeField] private float defaultMaxOrderWaitTime = 70f;

    [Header("Sipariş Bekleme - Bar UI")]
    [SerializeField] private NPCOrderWaitUI orderWaitUI;

    private Coroutine orderWaitCoroutine;

    // =========================================================
    // KAFEDEN AYRILMA
    // =========================================================

    [Header("Kafeden Ayrılma")]
    [SerializeField] private float minWaitBeforeLeaving = 4f;

    [SerializeField] private float maxWaitBeforeLeaving = 5f;

    private Coroutine waitBeforeLeaveCoroutine;

    // Bu ziyarette sipariş doğru mu teslim edildi?
    // Sadece doğruysa masadan kalkarken bahşiş bırakılır.
    private bool wasOrderCorrect = true;

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

        if (animator != null)
        {
            animator.SetBool(
                "IsMale",
                isMale
            );
        }

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

        if (animator != null)
        {
            animator.SetBool(
                "IsMale",
                isMale
            );
        }

        agent.isStopped = false;

        if (isCafeCustomer)
        {
            // =====================================================
            // KUYRUK ZATEN DOLUYSA HİÇ GİRİŞE GİTMEDEN ÇIKIŞA GİT
            // =====================================================

            if (queuedNPCs.Count >= queuePoints.Count)
            {
                Debug.Log(
                    $"{gameObject.name}: Kuyruk zaten dolu, " +
                    "kafeye girmeden direkt çıkışa gidiyor."
                );

                isCafeCustomer = false;

                GoToSpawnSideExit();

                return;
            }

            currentState =
                NPCState.GoingToEntrance;

            agent.SetDestination(
                cafeEntrancePoint.position
            );

            Debug.Log(
                $"{gameObject.name} kafe müşterisi olarak " +
                "CafeEntrancePoint'e gidiyor."
            );
        }
        else
        {
            GoToSpawnSideExit();

            Debug.Log(
                $"{gameObject.name} sokak NPC'si olarak " +
                "kendi tarafındaki çıkışa gidiyor."
            );
        }
    }

    // =========================================================
    // SPAWN BİLGİSİNİ AL
    // =========================================================

    public void InitializeSpawn(
    bool cafeCustomer,
    bool fromSpawnPoint1,
    NPCSpawner spawner)
    {
        isCafeCustomer =
            cafeCustomer;

        spawnedFromPoint1 =
            fromSpawnPoint1;

        npcSpawner =
            spawner;
    }


    // =========================================================
    // SAHNE NOKTALARINI BUL
    // =========================================================

    private void FindScenePoints()
    {
        if (cafeEntrancePoint == null)
        {
            cafeEntrancePoint =
                FindTransform(
                    "CafeEntrancePoint"
                );
        }

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

        if (spawnPoint1 == null)
        {
            spawnPoint1 =
                FindTransform(
                    "SpawnPoint_1",
                    "SpawnPoint1",
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
            case NPCState.GoingToEntrance:

                if (HasReachedDestination())
                {
                    TryJoinQueue();
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

            case NPCState.GoingToTable:

                if (HasReachedDestination())
                {
                    ArriveAtTable();
                }

                break;

            case NPCState.AtTable:

                break;

            case NPCState.StandingUpFromTable:

                break;

            case NPCState.GoingToCafeEntranceExit:

                if (HasReachedDestination())
                {
                    GoToRandomExitPoint();
                }

                break;

            case NPCState.GoingToExit:

                if (HasReachedDestination())
                {
                    Debug.Log(
                        $"{gameObject.name} çıkışa ulaştı ve yok oldu."
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
        Debug.Log(
            $"{gameObject.name}: " +
            $"Queue sayısı = {queuePoints.Count}"
        );

        if (queuedNPCs.Contains(this))
            return;

        if (!isCafeCustomer)
        {
            GoToSpawnSideExit();

            return;
        }

        if (queuedNPCs.Count >= queuePoints.Count)
        {
            Debug.Log(
                $"{gameObject.name}: " +
                "Queue dolu. Bu NPC sokak NPC'sine dönüyor."
            );

            isCafeCustomer = false;

            GoToSpawnSideExit();

            return;
        }

        if (npcSpawner != null)
        {
            bool registered =
                npcSpawner.RegisterCafeCustomer();

            if (!registered)
            {
                Debug.Log(
                    $"{gameObject.name}: " +
                    "Günlük müşteri limiti dolmuş. " +
                    "Sokak NPC'sine dönüşüyor."
                );

                isCafeCustomer = false;

                GoToSpawnSideExit();

                return;
            }

            cafeCustomerRegistered = true;
        }

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
            $"{assignedQueuePoint.name} | " +
            "GERÇEK KAFE MÜŞTERİSİ"
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

            if (i < queuePoints.Count)
            {
                npc.MoveForwardInQueue(
                    queuePoints[i]
                );
            }
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

        if (customerOrder == null)
        {
            CreateCustomerOrder();
        }

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
    // SPAWN OLDUĞU TARAFTAKİ ÇIKIŞA GİT
    // =========================================================

    private void GoToSpawnSideExit()
    {
        Transform selectedExit;

        if (spawnedFromPoint1)
        {
            selectedExit =
                exitPoint1;
        }
        else
        {
            selectedExit =
                exitPoint2;
        }

        if (selectedExit == null)
        {
            Debug.LogError(
                $"{gameObject.name}: " +
                "Spawn tarafına ait ExitPoint bulunamadı!"
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
            $"Spawn tarafına ait çıkışa gidiyor → " +
            $"{selectedExit.name}"
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

        customerOrder.coffeeType =
            unlockedCoffees[
                Random.Range(
                    0,
                    unlockedCoffees.Count
                )
            ];

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

        customerOrder.reward =
            0;

        customerOrder.timeLimit =
            90f;

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
        else
        {
            customerOrder.espressoShot =
                EspressoShotButtonUI.ShotType.Single;
        }

        CreateRandomExtra();

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

        assignedSeat =
            SeatManager.Instance.GetFreeSeat();

        if (assignedSeat == null)
        {
            Debug.LogWarning(
                $"{gameObject.name} için boş sandalye bulunamadı."
            );

            return;
        }

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

        RemoveFromQueue();

        assignedDrinkPlacePoint.SetCustomer(
            this
        );

        Debug.Log(
            $"{gameObject.name} bağlantısı kuruldu: " +
            $"{assignedSeat.name} → " +
            $"{assignedDrinkPlacePoint.gameObject.name}"
        );

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

        // =====================================================
        // SİPARİŞ ÖZETİNİ UI'A YAZ
        // =====================================================

        string orderSummary =
            BuildOrderSummaryText();

        Debug.Log(
            $"{gameObject.name}: Sipariş özeti UI'a yazılıyor → " +
            $"'{orderSummary}'"
        );

        if (orderWaitUI != null)
        {
            orderWaitUI.SetOrderText(
                orderSummary
            );
        }
        else
        {
            Debug.LogWarning(
                $"{gameObject.name}: orderWaitUI atanmamış, " +
                "sipariş yazısı gösterilemiyor!"
            );
        }

        // =====================================================
        // SİPARİŞ BEKLEME ZAMANLAYICISINI BAŞLAT
        // =====================================================

        if (orderWaitCoroutine != null)
        {
            StopCoroutine(
                orderWaitCoroutine
            );
        }

        orderWaitCoroutine =
            StartCoroutine(
                WaitForOrderTimeout()
            );

        Debug.Log(
            $"{gameObject.name} masaya ulaştı."
        );
    }

    // =========================================================
    // SİPARİŞ ÖZET YAZISI OLUŞTUR (BARIN ALTINDA GÖRÜNÜR)
    // =========================================================

    private string BuildOrderSummaryText()
    {
        if (customerOrder == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: BuildOrderSummaryText çağrıldı " +
                "ama customerOrder null!"
            );

            return "";
        }

        string coffeePart;

        if (customerOrder.coffeeType ==
            CoffeeType.Espresso)
        {
            string shotName =
                customerOrder.espressoShot ==
                EspressoShotButtonUI.ShotType.Single
                    ? "Tek Shot"
                    : "Double Shot";

            coffeePart =
                $"Espresso ({shotName})";
        }
        else
        {
            string sizeName =
                customerOrder.size switch
                {
                    CupSize.Small => "Küçük",
                    CupSize.Medium => "Orta",
                    CupSize.Large => "Büyük",
                    _ => ""
                };

            coffeePart =
                $"{sizeName} {customerOrder.coffeeType}";
        }

        if (customerOrder.requestedExtras != null &&
            customerOrder.requestedExtras.Count > 0)
        {
            coffeePart +=
                " + " +
                string.Join(
                    ", ",
                    customerOrder.requestedExtras
                );
        }

        return coffeePart;
    }

    // =========================================================
    // KAHVE TÜRÜNE GÖRE BEKLEME SÜRESİNİ BUL
    // =========================================================

    private float GetWaitTimeForOrder()
    {
        if (customerOrder != null &&
            coffeeWaitTimes != null)
        {
            CoffeeWaitTime match =
                coffeeWaitTimes.Find(
                    c => c.coffeeType ==
                         customerOrder.coffeeType
                );

            if (match != null)
            {
                return Random.Range(
                    match.minWaitTime,
                    match.maxWaitTime
                );
            }
        }

        return Random.Range(
            defaultMinOrderWaitTime,
            defaultMaxOrderWaitTime
        );
    }

    // =========================================================
    // SİPARİŞ ZAMAN AŞIMI BEKLEME DÖNGÜSÜ (BAR GÜNCELLEMELİ)
    // =========================================================

    private IEnumerator WaitForOrderTimeout()
    {
        float waitTime =
            GetWaitTimeForOrder();

        float elapsed = 0f;

        if (orderWaitUI != null)
        {
            orderWaitUI.Show();

            orderWaitUI.UpdateProgress(
                0f,
                waitTime
            );
        }

        while (elapsed < waitTime)
        {
            elapsed += Time.deltaTime;

            float progress =
                elapsed / waitTime;

            float remaining =
                waitTime - elapsed;

            if (orderWaitUI != null)
            {
                orderWaitUI.UpdateProgress(
                    progress,
                    remaining
                );
            }

            yield return null;
        }

        orderWaitCoroutine = null;

        HandleOrderTimeout();
    }

    // =========================================================
    // SİPARİŞ ZAMANI DOLDU, KAHVE GELMEDİ
    // =========================================================

    private void HandleOrderTimeout()
    {
        if (currentState !=
            NPCState.AtTable)
        {
            return;
        }

        if (orderWaitUI != null)
        {
            orderWaitUI.Hide();
        }

        Debug.Log(
            $"{gameObject.name}: Sipariş süresi doldu, " +
            "kahve gelmediği için müşteri kalkıp gidiyor."
        );

        // Kahve gelmediği için bahşiş bırakılmayacak.
        wasOrderCorrect = false;

        // =====================================================
        // SİPARİŞ FİYATI KADAR CEZA
        // =====================================================

        float penalty =
            customerOrder != null
                ? customerOrder.pricePaid
                : 0f;

        if (penalty > 0f)
        {
            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.SubtractMoneyWithFeedback(
                    penalty
                );

                Debug.Log(
                    $"Sipariş zaman aşımı cezası: " +
                    $"-{penalty:0.00}$"
                );
            }
            else
            {
                Debug.LogWarning(
                    "MoneyManager.Instance bulunamadı! " +
                    "Zaman aşımı cezası uygulanamadı."
                );
            }
        }

        LeaveTable();
    }

    // =========================================================
    // SİPARİŞ TESLİM EDİLDİ (DOĞRU YA DA YANLIŞ)
    // =========================================================
    //
    // DrinkPlacePoint, kahveyi masaya koyduktan sonra bu metodu
    // çağırır. Doğruysa içme döngüsü başlar. Yanlışsa müşteri
    // içmeden bir süre sonra kalkıp gider.
    //
    // =========================================================

    public void ServeOrder(bool isCorrect, PickupItem cup)
    {
        if (currentState != NPCState.AtTable)
            return;

        // =====================================================
        // KAHVE GELDİ, SİPARİŞ ZAMAN AŞIMINI İPTAL ET
        // =====================================================

        if (orderWaitCoroutine != null)
        {
            StopCoroutine(
                orderWaitCoroutine
            );

            orderWaitCoroutine = null;
        }

        if (orderWaitUI != null)
        {
            orderWaitUI.Hide();
        }

        wasOrderCorrect = isCorrect;

        if (isCorrect)
        {
            StartDrinkSequence(cup);
        }
        else
        {
            Debug.Log(
                $"{gameObject.name}: Yanlış sipariş teslim edildi. " +
                "Müşterinin gerçek sipariş fiyatı kadar para kesiliyor."
            );

            // =====================================================
            // YANLIŞ SİPARİŞ CEZASI
            // =====================================================

            float wrongOrderPenalty =
                customerOrder != null
                    ? customerOrder.pricePaid
                    : 0f;

            if (wrongOrderPenalty > 0f)
            {
                if (MoneyManager.Instance != null)
                {
                    MoneyManager.Instance.SubtractMoneyWithFeedback(
                        wrongOrderPenalty
                    );

                    Debug.Log(
                        $"Yanlış sipariş cezası: " +
                        $"-{wrongOrderPenalty:0.00}$"
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "MoneyManager.Instance bulunamadı! " +
                        "Yanlış sipariş cezası uygulanamadı."
                    );
                }
            }
            else
            {
                Debug.LogWarning(
                    $"{gameObject.name}: " +
                    "Müşteri sipariş fiyatı 0 olduğu için ceza uygulanmadı."
                );
            }

            // =====================================================
            // MÜŞTERİ BEKLEMEDEN KALKACAK
            // =====================================================

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
            // Sadece doğru sipariş teslim edildiyse
            // masada bahşiş bırakılır.
            if (wasOrderCorrect)
            {
                assignedDrinkPlacePoint.SpawnTip();
            }

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
        if (orderWaitCoroutine != null)
        {
            StopCoroutine(
                orderWaitCoroutine
            );

            orderWaitCoroutine = null;
        }

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

        // NOT: queuePoints artık burada temizlenmiyor.
    }
}