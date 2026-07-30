using UnityEngine;

public class PortafilterPickup : MonoBehaviour
{
    [SerializeField] private GameObject groundCoffee;

    public bool HasGroundCoffee { get; private set; }

    private void Start()
    {
        groundCoffee.SetActive(false);
    }

    public void FillWithCoffee()
    {
        if (HasGroundCoffee)
            return;

        HasGroundCoffee = true;
        groundCoffee.SetActive(true);
    }
}