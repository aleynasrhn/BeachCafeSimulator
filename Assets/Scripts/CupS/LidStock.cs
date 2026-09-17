using UnityEngine;

public class LidStock : MonoBehaviour
{
    [Header("Stok Ayarları")]
    [SerializeField] private PickupItem pickupLid;
    [SerializeField] private GameObject lidPrefab;

    private bool waitingForPickup = false;

    // Yığının orijinal, düzgün pozisyon/rotasyonu - bir kere kaydedilir,
    // asla değişmez. Yeni kapaklar hep buraya spawn olur.
    private Vector3 stockPosition;
    private Quaternion stockRotation;

    private void Start()
    {
        if (pickupLid != null)
        {
            stockPosition = pickupLid.transform.position;
            stockRotation = pickupLid.transform.rotation;
        }
    }

    private void Update()
    {
        if (pickupLid == null || lidPrefab == null)
            return;

        // Üstteki kapak alındıysa
        if (pickupLid.IsHeld && !waitingForPickup)
        {
            waitingForPickup = true;

            SpawnReplacement();
        }
    }

    private void SpawnReplacement()
    {
        // pickupLid'in o anki (belki elin yönüne doğru kaymış)
        // transform'unu değil, sabit stockPosition/stockRotation'ı kullanıyoruz.
        GameObject newLid = Instantiate(
            lidPrefab,
            stockPosition,
            stockRotation
        );

        // Yeni kapağı fiziksel olarak stokta sabitle
        Rigidbody rb = newLid.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Yeni kapağı artık alınabilir kapak yap
        PickupItem newPickup = newLid.GetComponent<PickupItem>();

        if (newPickup != null)
        {
            pickupLid = newPickup;
        }

        waitingForPickup = false;
    }
}