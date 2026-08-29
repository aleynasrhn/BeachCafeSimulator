using UnityEngine;

public class CupStock : MonoBehaviour
{
    [Header("Stok Ayarları")]
    [SerializeField] private PickupItem pickupCup;
    [SerializeField] private GameObject cupPrefab;

    private bool waitingForPickup = false;

    // Yığının orijinal, düzgün pozisyon/rotasyonu - bir kere kaydedilir,
    // asla değişmez. Yeni bardaklar hep buraya spawn olur.
    private Vector3 stockPosition;
    private Quaternion stockRotation;

    private void Start()
    {
        if (pickupCup != null)
        {
            stockPosition = pickupCup.transform.position;
            stockRotation = pickupCup.transform.rotation;
        }
    }

    private void Update()
    {
        if (pickupCup == null || cupPrefab == null)
            return;

        // Üstteki bardak alındıysa
        if (pickupCup.IsHeld && !waitingForPickup)
        {
            waitingForPickup = true;

            SpawnReplacement();
        }
    }

    private void SpawnReplacement()
    {
        // Artık pickupCup'ın o anki (belki elin yönüne doğru kaymış)
        // transform'unu değil, sabit stockPosition/stockRotation'ı kullanıyoruz.
        GameObject newCup = Instantiate(
            cupPrefab,
            stockPosition,
            stockRotation
        );

        // Yeni bardağı fiziksel olarak stokta sabitle
        Rigidbody rb = newCup.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Yeni bardağı artık alınabilir bardak yap
        PickupItem newPickup = newCup.GetComponent<PickupItem>();

        if (newPickup != null)
        {
            pickupCup = newPickup;
        }

        waitingForPickup = false;
    }
}