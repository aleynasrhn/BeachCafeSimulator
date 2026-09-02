using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    [Header("NPC Noktaları")]
    [SerializeField] private Transform cafeEntrancePoint;
    [SerializeField] private Transform queuePoint;

    private Transform assignedSeat;

    // NPC'nin bağlı olduğu kahve bırakma noktası
    private DrinkPlacePoint assignedDrinkPlacePoint;

    private NavMeshAgent agent;
    private Animator animator;

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
        AtTable
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


    // =========================================================
    // HEDEFE ULAŞTI MI?
    // =========================================================

    private bool HasReachedDestination()
    {
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
            // Espresso daima küçük.

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

        customerOrder.reward = 0;


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
            // Espresso kendi başına
            // Single veya Double olabilir.

            customerOrder.espressoShot =
                Random.value > 0.5f
                    ? EspressoShotButtonUI.ShotType.Single
                    : EspressoShotButtonUI.ShotType.Double;
        }
        else
        {
            // Normal kahveler başlangıçta her zaman Single.

            customerOrder.espressoShot =
                EspressoShotButtonUI.ShotType.Single;
        }


        // =====================================================
        // EKSTRA
        // =====================================================

        CreateRandomExtra();


        // =====================================================
        // EKSTRA ESPRESSO KURALI
        // =====================================================

        // Normal bir kahve ekstra espresso istiyorsa
        // bu sipariş Double shot olarak hazırlanmalı.

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
        // SİPARİŞİ KASA EKRANINA GÖNDER
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
                // Espresso zaten Single / Double seçiyor.
                // Ekstra Espresso istemeyecek.

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


        // Hiç açık ekstra yoksa

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
        // KARŞILIK GELEN DRINK PLACE POINTİ BUL
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
        // DRINK PLACE POINTİ BU NPC'YE BAĞLA
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
            NPCState.AtTable)
        {
            animator.SetFloat(
                "Speed",
                0f
            );

            return;
        }


        animator.SetFloat(
            "Speed",
            agent.velocity.magnitude
        );
    }



}