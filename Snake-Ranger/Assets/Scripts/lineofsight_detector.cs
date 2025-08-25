using UnityEngine;

public class LineOfSightDetector : MonoBehaviour
{
    [SerializeField]
    private LayerMask m_playerLayerMask;
    [SerializeField]
    private float m_detectionRange = 10.0f;
    [SerializeField]
    private float m_detectionHeight = 3f;

    [SerializeField] private bool showDebugVisuals = true;

    public GameObject PerformDetection(GameObject potentialTarget)
    {
        RaycastHit hit;
        Vector3 direction = potentialTarget.transform.position - transform.position;
        Physics.Raycast(transform.position + Vector3.up * m_detectionHeight,
            direction, out hit, m_detectionRange, m_playerLayerMask);

        if (hit.collider != null && hit.collider.gameObject == potentialTarget)
        {
            if (showDebugVisuals && this.enabled)
            {
                Debug.DrawLine(transform.position + Vector3.up * m_detectionHeight,
                    potentialTarget.transform.position, Color.green);
            }
            return hit.collider.gameObject;
        }
        else
        {
            return null;
        }
    }

    private void OnDrawGizmos()
    {
        if (showDebugVisuals)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + Vector3.up * m_detectionHeight, 0.3f);
        }
    }
}
