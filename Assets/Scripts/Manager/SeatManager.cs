using UnityEngine;
using System.Collections.Generic;

public class SeatManager : MonoBehaviour
{
    public static SeatManager Instance;

    [Header("Oturma Noktaları (CustomerSeat objeleri)")]
    [SerializeField] private Transform[] sitPoints;

    private readonly HashSet<Transform> occupiedSeats =
        new HashSet<Transform>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Transform GetFreeSeat()
    {
        List<Transform> freeSeats = new List<Transform>();

        foreach (var seat in sitPoints)
        {
            if (!occupiedSeats.Contains(seat))
            {
                freeSeats.Add(seat);
            }
        }

        if (freeSeats.Count == 0)
        {
            Debug.LogWarning("Boş sandalye kalmadı!");
            return null;
        }

        Transform chosen =
            freeSeats[Random.Range(0, freeSeats.Count)];

        occupiedSeats.Add(chosen);

        return chosen;
    }

    public void ReleaseSeat(Transform seat)
    {
        if (seat != null)
        {
            occupiedSeats.Remove(seat);
        }
    }
}