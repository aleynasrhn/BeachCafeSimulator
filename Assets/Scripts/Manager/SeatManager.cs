using UnityEngine;
using System.Collections.Generic;

public class SeatManager : MonoBehaviour
{
    public static SeatManager Instance;

    [Header("Oturma Noktaları (CustomerSeat objeleri)")]
    [SerializeField] private Transform[] sitPoints;

    [Header("Kahve Bırakma Noktaları")]
    [SerializeField] private DrinkPlacePoint[] drinkPlacePoints;

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
            return;
        }
    }


    // =========================================================
    // BOŞ KOLTUK BUL
    // =========================================================

    public Transform GetFreeSeat()
    {
        List<Transform> freeSeats =
            new List<Transform>();

        foreach (Transform seat in sitPoints)
        {
            if (seat == null)
                continue;

            if (!occupiedSeats.Contains(seat))
            {
                freeSeats.Add(seat);
            }
        }

        if (freeSeats.Count == 0)
        {
            Debug.LogWarning(
                "Boş sandalye kalmadı!"
            );

            return null;
        }

        Transform chosen =
            freeSeats[
                Random.Range(
                    0,
                    freeSeats.Count
                )
            ];

        occupiedSeats.Add(chosen);

        return chosen;
    }


    // =========================================================
    // KOLTUĞU BOŞALT
    // =========================================================

    public void ReleaseSeat(Transform seat)
    {
        if (seat != null)
        {
            occupiedSeats.Remove(seat);
        }
    }


    // =========================================================
    // DRINK PLACE POINT BUL
    // =========================================================

    public DrinkPlacePoint GetDrinkPlacePoint(
        Transform seat)
    {
        if (seat == null)
            return null;


        string seatName =
            seat.name;


        // Örnek:
        // SitPoint1
        // SitPoint15
        //
        // DrinkPlacePoint1
        // DrinkPlacePoint15

        if (!seatName.StartsWith("SitPoint"))
        {
            Debug.LogWarning(
                $"Geçersiz SitPoint adı: {seatName}"
            );

            return null;
        }


        string number =
            seatName.Replace(
                "SitPoint",
                ""
            );


        string targetName =
            "DrinkPlacePoint" +
            number;


        foreach (
            DrinkPlacePoint point
            in drinkPlacePoints)
        {
            if (point == null)
                continue;


            if (point.gameObject.name ==
                targetName)
            {
                return point;
            }
        }


        Debug.LogWarning(
            $"'{targetName}' bulunamadı."
        );


        return null;
    }
}