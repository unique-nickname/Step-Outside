using System;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public Transform target;
    [SerializeField] private float smoothFactor = 12f;


    private void FixedUpdate()
    {
        if (target == null) return;
        Vector3 desiredPosition = target.position;
        desiredPosition.z = transform.position.z; // Keep the camera's z position unchanged
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothFactor * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}
