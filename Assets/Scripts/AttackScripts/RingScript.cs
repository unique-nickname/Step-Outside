using UnityEngine;

public class RingScript : MonoBehaviour
{

    public float rotationSpeed;
    private float currentRotation;

    private void FixedUpdate()
    {
        currentRotation += rotationSpeed / 60f;
        if (currentRotation >= 360f) {
            currentRotation = 0f;
        }
        transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);

        if (transform.childCount == 0) {
            Destroy(gameObject);
        }
    }

}
