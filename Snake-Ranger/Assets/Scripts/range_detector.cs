using UnityEngine;

public class RangeDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private LayerMask detectionMask;
    [SerializeField] private bool showDebugVisuals = true;

    public GameObject DetectedTarget
    {
        get;
        set;
    }

    public GameObject UpdateDetector()
    {
        // Perform sphere check
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, detectionMask);

        if (colliders.Length > 0)
        {
            DetectedTarget = colliders[0].gameObject;
        }
        else
        {
            DetectedTarget = null;
        }
        return DetectedTarget;
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        if (!showDebugVisuals || this.enabled == false) return;

        Gizmos.color = DetectedTarget ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

    }
}
