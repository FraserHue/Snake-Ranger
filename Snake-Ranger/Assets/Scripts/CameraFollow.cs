using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        // the snake head
    public Vector3 offset = new Vector3(0, 10, -10);
    public float smoothSpeed = 5f;  // smoothness of camera motion

    void LateUpdate()
    {
        if (!target) return;

        // follow snake's position only
        Vector3 desiredPos = target.position + offset;

        // interpolate smoothly towards desired position
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);

    }
}