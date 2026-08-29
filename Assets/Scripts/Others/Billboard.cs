using UnityEngine;

/// <summary>
/// World Space Canvas'a eklenir - her frame kameraya dönük durmasını sağlar,
/// böylece oyuncu hangi açıdan bakarsa baksın yazı okunabilir kalır.
/// </summary>
public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;

        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                          mainCamera.transform.rotation * Vector3.up);
    }
}