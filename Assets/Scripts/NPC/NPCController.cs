using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    [Header("NPC Noktaları")]
    [SerializeField] private Transform cafeEntrancePoint;
    [SerializeField] private Transform queuePoint;


    // =========================================================
    // ÇIKIŞ NOKTALARI
    // =========================================================

    [Header("Çıkış Noktaları")]
    [SerializeField] private Transform exitPoint1;
    [SerializeField] private Transform exitPoint2;


    private Transform assignedSeat;

    // NPC'nin bağlı olduğu kahve bırakma noktası
    private DrinkPlacePoint assignedDrinkPlacePoint;

    private NavMeshAgent agent;
    private Animator animator;


    // =========================================================
    // İÇME
    // =========================================================
    //
    // Bardağın elde nasıl konumlanacağı, küçüleceği ve
    // masaya bırakılması NPCDrinkCupController tarafından
    // yönetiliyor.
    //
    // Bu script sadece:
    // - hangi bardağın içileceğini bildirir
    // - içme döngüsünü yönetir
    //
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

        // NPC kalkma animasyonunu oynatıyor
        StandingUpFromTable,

        // Kalktıktan sonra CafeEntrancePoint'e gidiyor
        GoingToCafeEntranceExit,

        // CafeEntrancePoint'ten seçilen çıkışa gidiyor
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


        // Inspector'da atanmadıysa otomatik bul
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
        if (cafeEntrancePoint == null)
        {
            Debug.LogError(
                "CafeEntrancePoint atanmadı!"
            );

            return;
        }


        if (queuePoint == null)
        {
            Debug.LogError(
                "QueuePoint atanmadı!"
            );

            return;
        }


        agent.isStopped = false;

        currentState =
            NPCState.GoingToEntrance;


        agent.SetDestination(
            cafeEntrancePoint.position
        );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        switch (currentState)
        {
            // =================================================
            // GİRİŞTEN KASAYA
            // =================================================

            case NPCState.GoingToEntrance:

                if (HasReachedDestination())
                {
                    currentState =
                        NPCState.GoingToQueue;

                    agent.isStopped = false;

                    agent.SetDestination(
                        queuePoint.position
                    );
                }

                break;


            // =================================================
            // KASADA BEKLE
            // =================================================

            case NPCState.GoingToQueue:

                if (HasReachedDestination())
                {
                    StopAtQueue();

                    CreateCustomerOrder();
                }

                break;


            // =================================================
            // SİPARİŞ ONAYI BEKLE
            // =================================================

            case NPCState.Waiting:

                break;


            // =================================================
            // MASAYA GİT
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

                // Doğru kahve teslim edilmesini bekliyor.

                break;


            // =================================================
            // KALKMA ANİMASYONU
            // =================================================

            case NPCState.StandingUpFromTable:

                // Burada NPC henüz hareket etmiyor.
                // StandingUp animasyonu devam ediyor.

                break;


            // =================================================
            // CAFE ENTRANCE'A GİDİYOR
            // =================================================

            case NPCState.GoingToCafeEntranceExit:

                if (HasReachedDestination())
                {
                    GoToRandomExitPoint();
                }

                break;


            // =================================================
            // SEÇİLEN ÇIKIŞA GİDİYOR
            // =================================================

            case NPCState.GoingToExit:

                if (HasReachedDestination())
                {
                    Debug.Log(
                        $"{gameObject.name} kafeden çıktı ve kayboldu."
                    );

                    Destroy(gameObject);
                }

                break;
        }


        UpdateAnimation();

        HandleDrinkSequence();
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
    // KASADA DUR
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


        Debug.Log(
            $"{gameObject.name} QueuePoint'e ulaştı ve bekliyor."
        );
    }


    // =========================================================
    // MÜŞTERİ SİPARİŞİ OLUŞTUR
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


        // =====================================================
        // SİPARİŞ EKRANI
        // =====================================================

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
    // AÇIK KAHVELERİ GETİR
    // =========================================================

    private List<CoffeeType> GetUnlockedCoffeeTypes()
    {
        List<CoffeeType> result =
            new List<CoffeeType>();


        // UnlockManager yoksa güvenli varsayılanlar

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
    // RASTGELE EKSTRA OLUŞTUR
    // =========================================================

    private void CreateRandomExtra()
    {
        // %50 ihtimalle hiç ekstra istemesin.

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
            // =================================================
            // ESPRESSO ÖZEL KURALI
            // =================================================

            if (customerOrder.coffeeType ==
                CoffeeType.Espresso &&
                extra == "Ekstra Espresso")
            {
                continue;
            }


            // =================================================
            // GÜN KONTROLÜ
            // =================================================

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
        // BOŞ SANDALYE AL
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

            return;
        }


        // =====================================================
        // DRINK PLACE POINT'İ BU NPC'YE BAĞLA
        // =====================================================

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
    // SANDALYEYE ULAŞTI
    // =========================================================

    private void ArriveAtTable()
    {
        currentState =
            NPCState.AtTable;


        agent.isStopped = true;

        agent.ResetPath();


        // NPC'yi tam sandalyeye oturt.
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


        // =====================================================
        // BAĞLANTI KONTROLÜ
        // =====================================================

        if (assignedDrinkPlacePoint != null &&
            customerOrder != null)
        {
            Debug.Log(
                $"{gameObject.name} artık " +
                $"{assignedDrinkPlacePoint.gameObject.name} " +
                "üzerinden sipariş teslim alabilir."
            );
        }
    }


    // =========================================================
    // DOĞRU KAHVE GELDİĞİNDE İÇMEYİ BAŞLAT
    // =========================================================

    public void StartDrinkSequence(PickupItem cup)
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
                $"{gameObject.name} için NPCDrinkCupController bulunamadı."
            );

            return;
        }


        // =====================================================
        // BARDAĞI DRINK CUP CONTROLLER'A DEVRET
        // =====================================================

        drinkCupController.SetCup(
            cup
        );


        // =====================================================
        // İÇMEYİ BAŞLAT
        // =====================================================

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


        // =====================================================
        // DRINKING STATE'İNE YENİ GİRDİ
        // =====================================================

        if (inDrinkState &&
            !wasInDrinkState)
        {
            drinkCount++;


            Debug.Log(
                $"{gameObject.name} içme animasyonu: " +
                $"{drinkCount}/{totalDrinkCycles}"
            );
        }


        // =====================================================
        // DRINKING STATE'İNDEN ÇIKTI
        // =====================================================

        if (!inDrinkState &&
            wasInDrinkState)
        {
            if (drinkCount < totalDrinkCycles)
            {
                // Bir sonraki içme öncesi bekle.

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
                // =================================================
                // TÜM İÇME DÖNGÜLERİ BİTTİ
                // =================================================

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
    // BEKLE, SONRA TEKRAR İÇ
    // =========================================================

    private System.Collections.IEnumerator WaitThenDrinkAgain()
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
    // BEKLE, SONRA MASADAN KALK
    // =========================================================

    private System.Collections.IEnumerator WaitThenLeaveTable()
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
    //
    // BURADA NPC HENÜZ YÜRÜMEYE BAŞLAMAZ.
    // Sadece StandingUp animasyonunu başlatır.
    //
    // NavMeshAgent:
    // StandingUp animasyonu bittikten sonra
    // Animation Event ile açılacak.
    //
    // =========================================================

    private void LeaveTable()
    {
        if (cafeEntrancePoint == null)
        {
            Debug.LogError(
                $"{gameObject.name}: CafeEntrancePoint atanmadı!"
            );

            return;
        }


        // =====================================================
        // KALKMA DURUMUNA GEÇ
        // =====================================================

        currentState =
            NPCState.StandingUpFromTable;


        // =====================================================
        // OTURMA ANİMASYONUNDAN ÇIK
        // =====================================================

        if (animator != null)
        {
            animator.SetBool(
                "IsSitting",
                false
            );
        }


        // =====================================================
        // SANDALYEYİ BOŞALT
        // =====================================================

        if (SeatManager.Instance != null &&
            assignedSeat != null)
        {
            SeatManager.Instance.ReleaseSeat(
                assignedSeat
            );
        }


        // =====================================================
        // DRINK PLACE POINT'İ BOŞALT
        // =====================================================

        if (assignedDrinkPlacePoint != null)
        {
            assignedDrinkPlacePoint.SetCustomer(
                null
            );
        }


        // =====================================================
        // DİKKAT:
        // AGENT BURADA AÇILMIYOR!
        // =====================================================

        Debug.Log(
            $"{gameObject.name} masadan kalkıyor."
        );
    }


    // =========================================================
    // KALKMA ANİMASYONU BİTTİ
    // =========================================================
    //
    // BU METODU "StandingUp" ANİMASYONUNUN
    // SONUNDAKİ ANIMATION EVENT ÇAĞIRACAK.
    //
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
                $"{gameObject.name}: NavMeshAgent bulunamadı!"
            );

            return;
        }


        if (cafeEntrancePoint == null)
        {
            Debug.LogError(
                $"{gameObject.name}: CafeEntrancePoint atanmadı!"
            );

            return;
        }


        // =====================================================
        // NAV MESH AGENT'İ AÇ
        // =====================================================

        agent.enabled = true;


        agent.Warp(
            transform.position
        );


        agent.isStopped = false;


        // =====================================================
        // CAFE ENTRANCE'A YÜRÜ
        // =====================================================

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
    // RASTGELE ÇIKIŞ NOKTASI SEÇ
    // =========================================================

    private void GoToRandomExitPoint()
    {
        if (exitPoint1 == null ||
            exitPoint2 == null)
        {
            Debug.LogError(
                $"{gameObject.name}: " +
                "ExitPoint1 veya ExitPoint2 atanmadı!"
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


        // =====================================================
        // SEÇİLEN ÇIKIŞA GİT
        // =====================================================

        currentState =
            NPCState.GoingToExit;


        agent.SetDestination(
            selectedExit.position
        );


        Debug.Log(
            $"{gameObject.name} CafeEntrancePoint'e ulaştı. " +
            $"Seçilen çıkış: {selectedExit.name}"
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


        // =====================================================
        // OTURAN NPC
        // =====================================================

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


        // =====================================================
        // YÜRÜYEN NPC
        // =====================================================

        animator.SetFloat(
            "Speed",
            agent.enabled
                ? agent.velocity.magnitude
                : 0f
        );
    }
}   