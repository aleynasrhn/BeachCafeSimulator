using UnityEngine;

public class EspressoShotButtonUI : MonoBehaviour
{
    public enum ShotType
    {
        Single,
        Double
    }

    [SerializeField] private ShotType shotType;

    public ShotType Shot => shotType;
}