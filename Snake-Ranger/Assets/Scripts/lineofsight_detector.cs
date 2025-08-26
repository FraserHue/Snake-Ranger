using UnityEngine;

public class LineOfSightDetector : MonoBehaviour
{
    [SerializeField]
    private LayerMask m_playerLayerMask;
    [SerializeField]
    private float m_detectionRange = 10.0f;
    [SerializeField]
    private float m_detectionHeight = 3f;
    [SerializeField] private float m_loseSightGrace = 0.35f;
    [SerializeField] private bool showDebugVisuals = true;

    float _lastSeenTime = -999f;

    public GameObject PerformDetection(GameObject potentialTarget)
    {
        if (!potentialTarget) return null;

        Vector3 eye = transform.position + Vector3.up * m_detectionHeight;
        Vector3 tgt = potentialTarget.transform.position;
        float dist = Vector3.Distance(eye, tgt);
        if (dist > m_detectionRange) { Draw(eye, tgt, Color.red); return SeenRecently() ? potentialTarget : null; }

        // linecast from eye to target using one mask (player + obstacles). if first hit is player, los is clear.
        if (Physics.Linecast(eye, tgt, out var hit, m_playerLayerMask, QueryTriggerInteraction.Ignore)
            && hit.collider && hit.collider.gameObject == potentialTarget)
        {
            _lastSeenTime = Time.time;
            Draw(eye, tgt, Color.green);
            return potentialTarget;
        }

        Draw(eye, tgt, Color.yellow); // blocked this frame
        return SeenRecently() ? potentialTarget : null; // small grace to prevent stutter
    }

    bool SeenRecently() => Time.time - _lastSeenTime <= m_loseSightGrace;

    void Draw(Vector3 a, Vector3 b, Color c)
    {
        if (showDebugVisuals && enabled) Debug.DrawLine(a, b, c);
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
