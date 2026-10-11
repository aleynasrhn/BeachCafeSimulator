
using System.Collections;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    [System.Serializable]
    public class TrafficLane
    {
        public string laneName;

        [Header("Spawn Settings")]
        public Transform spawnPoint;
        public Transform turnPoint;
        public Transform destroyPoint;

        [Header("Car Prefabs")]
        public GameObject[] carPrefabs;

        [Header("Spawn Interval")]
        public float minSpawnInterval = 5f;
        public float maxSpawnInterval = 10f;

        [Header("Movement")]
        public float moveSpeed = 8f;
        public float turnSpeed = 180f;
    }

    [Header("Traffic Lanes")]
    [SerializeField]
    private TrafficLane lane1 = new TrafficLane
    {
        laneName = "Lane 1"
    };

    [SerializeField]
    private TrafficLane lane2 = new TrafficLane
    {
        laneName = "Lane 2"
    };

    private void Start()
    {
        StartCoroutine(SpawnCars(lane1));
        StartCoroutine(SpawnCars(lane2));
    }

    private IEnumerator SpawnCars(TrafficLane lane)
    {
        while (true)
        {
            float waitTime = Random.Range(
                lane.minSpawnInterval,
                lane.maxSpawnInterval
            );

            yield return new WaitForSeconds(waitTime);

            if (lane.spawnPoint == null ||
                lane.turnPoint == null ||
                lane.destroyPoint == null ||
                lane.carPrefabs == null ||
                lane.carPrefabs.Length == 0)
            {
                continue;
            }

            GameObject prefab = lane.carPrefabs[
                Random.Range(0, lane.carPrefabs.Length)
            ];

            if (prefab == null)
                continue;

            Quaternion spawnRotation = lane.spawnPoint.rotation;

            if (prefab.name.ToLower().Contains("merco"))
            {
                spawnRotation *= Quaternion.Euler(-90f, 0f, 0f);
            }

            GameObject car = Instantiate(
                prefab,
                lane.spawnPoint.position,
                spawnRotation
            );

            StartCoroutine(MoveCar(car.transform, lane));
        }
    }

    private IEnumerator MoveCar(Transform car, TrafficLane lane)
    {
        // Dönüş noktasına kadar ilerle.
        while (car != null &&
               Vector3.Distance(car.position, lane.turnPoint.position) > 0.2f)
        {
            Vector3 direction = lane.turnPoint.position - car.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(direction);

                car.rotation = Quaternion.RotateTowards(
                    car.rotation,
                    targetRotation,
                    lane.turnSpeed * Time.deltaTime
                );
            }

            car.position = Vector3.MoveTowards(
                car.position,
                lane.turnPoint.position,
                lane.moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (car == null)
            yield break;

        // Dönüş noktasından yok olma noktasına ilerle.
        while (car != null &&
               Vector3.Distance(car.position, lane.destroyPoint.position) > 0.2f)
        {
            Vector3 direction = lane.destroyPoint.position - car.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(direction);

                car.rotation = Quaternion.RotateTowards(
                    car.rotation,
                    targetRotation,
                    lane.turnSpeed * Time.deltaTime
                );
            }

            car.position = Vector3.MoveTowards(
                car.position,
                lane.destroyPoint.position,
                lane.moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (car != null)
        {
            Destroy(car.gameObject);
        }
    }
}
