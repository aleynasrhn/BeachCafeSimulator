using UnityEngine;

/// <summary>
/// Elde tutulan eşyayı, hiçbir etkileşim objesine bakılmıyorken
/// E ile yere bırakmayı sağlar. Çöp poşeti ve kutular için.
/// PickupItem'a ve PlayerInteraction'a dokunmadan çalışır.
/// </summary>
[RequireComponent(typeof(PickupItem))]
public class DroppableByE : MonoBehaviour
{
    [Tooltip("Tezgâha bakılıp bakılmadığını anlamak için kullanılan ışın mesafesi.")]
    [SerializeField] private float rayDistance = 3f;

    private PickupItem pickup;
    private PlayerInteraction player;

    private bool wasHeld = false;
    private int heldSinceFrame = -1;

    private void Awake()
    {
        pickup = GetComponent<PickupItem>();
    }

    private void Update()
    {
        bool isHeld = pickup.IsHeld;

        // Eşya elimize alındığı kare: aynı E basışıyla hemen
        // bırakılmasın diye o kareyi atla.
        if (isHeld && !wasHeld)
        {
            heldSinceFrame = Time.frameCount;
        }

        wasHeld = isHeld;

        if (!isHeld)
            return;

        if (Time.frameCount == heldSinceFrame)
            return;

        if (!Input.GetKeyDown(KeyCode.E))
            return;

        if (Time.timeScale == 0f)
            return;

        if (ComputerInteraction.Instance != null &&
            ComputerInteraction.Instance.IsUsingComputer)
        {
            return;
        }

        if (PosMachineController.Instance != null &&
            PosMachineController.Instance.IsUsingPos)
        {
            return;
        }

        if (player == null)
        {
            player = FindFirstObjectByType<PlayerInteraction>();
        }

        if (player == null)
            return;

        // Bir etkileşim objesine (kova, masa, makine...) bakıyorsa
        // o obje kendi E işini yapsın.
        if (player.IsLookingAtInteractable)
            return;

        // Tezgâh yüzeyine bakıyorsa tezgâh kendi bırakma işini yapsın.
        if (IsLookingAtCounter())
            return;

        pickup.Drop(player);
    }

    private bool IsLookingAtCounter()
    {
        Camera cam = Camera.main;

        if (cam == null)
            return false;

        Ray ray =
            cam.ViewportPointToRay(
                new Vector3(0.5f, 0.5f, 0f)
            );

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            return hit.collider.TryGetComponent(
                out CounterSurface _
            );
        }

        return false;
    }
}