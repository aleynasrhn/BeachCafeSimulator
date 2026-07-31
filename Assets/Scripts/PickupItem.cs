using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour
{
    [Header("Hold Settings")]
    public Vector3 holdPosition;
    public Vector3 holdRotation;

    [Header("Place Settings")]
    public float placeHeight = 0.02f;
    public Vector3 placeRotation;

    [Header("Debug")]
    public bool livePreview = true;

    private Rigidbody rb;
    private Collider col;

    private bool isHeld;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void PickUp(Transform holdPoint)
    {
        isHeld = true;

        rb.isKinematic = true;
        col.enabled = false;

        transform.SetParent(holdPoint);

        ApplyHoldTransform();

        Debug.Log($"{gameObject.name} - Hold Position: {holdPosition}");
    }

    public void Place(Vector3 position)
    {
        isHeld = false;

        transform.SetParent(null);

        transform.position = position;
        transform.rotation = Quaternion.Euler(placeRotation);

        rb.isKinematic = true;
        col.enabled = true;
    }

    public void Drop()
    {
        isHeld = false;

        transform.SetParent(null);

        rb.isKinematic = false;
        col.enabled = true;
    }

    private void LateUpdate()
    {
        if (!livePreview || !isHeld)
            return;

        ApplyHoldTransform();
    }

    private void ApplyHoldTransform()
    {
        transform.localPosition = holdPosition;
        transform.localRotation = Quaternion.Euler(holdRotation);
    }
}