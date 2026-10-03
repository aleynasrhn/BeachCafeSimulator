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
    //
    // Bir koltuk şu durumda "boş" sayılmaz:
    // - Zaten başka bir NPC tarafından rezerve edilmişse
    // - Masasında (DrinkPlacePoint) hâlâ bardak veya bahşiş
    //   duruyorsa (IsOccupiedByItem). Oyuncu masayı temizlemeden
    //   yeni müşteri oraya oturamaz.
    //
    // =========================================================

    public Transform GetFreeSeat()
    {
        List<Transform> freeSeats =
            new List<Transform>();

        foreach (Transform seat in sitPoints)
        {
            if (seat == null)
                continue;

            if (occupiedSeats.Contains(seat))
                continue;

            DrinkPlacePoint point =
                GetDrinkPlacePoint(seat);

            if (point != null &&
                point.IsOccupiedByItem)
            {
                // Masada hâlâ kullanılmış bardak ya da
                // alınmamış bahşiş var, bu koltuğu atla.
                continue;
            }

            freeSeats.Add(seat);
        }

        if (freeSeats.Count == 0)
        {
            Debug.LogWarning(
                "Boş sandalye kalmadı! " +
                "(Hiç yok ya da hepsinin masası kirli olabilir.)"
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