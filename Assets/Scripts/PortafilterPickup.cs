using UnityEngine;

public class PortafilterPickup : MonoBehaviour
{
    private Transform holdPoint;
    private bool isHeld = false;

    private void Start()
    {
        GameObject point = GameObject.Find("HoldPoint");

        if (point != null)
        {
            holdPoint = point.transform;
        }
        else
        {
            Debug.LogError("HoldPoint bulunamadı!");
        }
    }

    public void PickUp()
    {
        if (isHeld || holdPoint == null)
            return;

        isHeld = true;

        transform.SetParent(holdPoint);

        transform.localPosition = Vector3.zero;

        // Portafiltrenin elde doğru duruşu
        transform.localRotation = Quaternion.Euler(-90f, 90f, 0f);
    }
}