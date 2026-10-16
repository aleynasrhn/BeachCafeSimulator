using UnityEngine;

/// <summary>
/// Kapı MESH'İNİN (collider'ın olduğu objenin) üzerine eklenir —
/// ToiletDoor'un olduğu DoorPivot'a DEĞİL. Oyuncu E'ye basınca
/// bağlı olduğu ToiletDoor'u açar/kapatır. Kilitliyken hiçbir şey
/// yapmaz.
/// </summary>
public class ToiletDoorInteractable : MonoBehaviour, IInteractable
{
    [Tooltip("Bu kapı mesh'inin ait olduğu DoorPivot'taki ToiletDoor.")]
    [SerializeField] private ToiletDoor door;

    public string GetInteractPrompt()
    {
        if (door == null)
            return "";

        if (door.IsLocked)
            return "Kapı kilitli (içeride biri var)";

        return door.IsOpen
            ? "Kapıyı kapat"
            : "Kapıyı aç";
    }

    public void Interact(PlayerInteraction player)
    {
        if (door == null)
            return;

        if (door.IsLocked || door.IsMoving)
            return;

        door.Toggle();
    }
}