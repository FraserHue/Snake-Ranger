using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    // detection fields (from your script)
    public GameObject PlayerTarget { get; private set; }
    [SerializeField] GameObject m_playerReference;
    [SerializeField] LayerMask m_playerLayerMask;   // set to player | obstacles
    [SerializeField] float m_detectionRange = 10f;
    [SerializeField] float eyeHeight = 1.0f;        // small fix: ray from eye level
    public Vector3 LastKnownPlayerPosition { get; private set; }

    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        LastKnownPlayerPosition = transform.position;
        // snap to navmesh once so movement is valid
        if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas)) agent.Warp(hit.position);
        if (!m_playerReference) { var p = GameObject.FindGameObjectWithTag("Player"); if (p) m_playerReference = p; }
    }

    void Update()
    {
        // raycast to player; walls block since obstacles are in the mask
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 dir = (m_playerReference.transform.position - origin).normalized;
        float dist = Mathf.Min(Vector3.Distance(origin, m_playerReference.transform.position), m_detectionRange);
        PlayerTarget = (Physics.Raycast(origin, dir, out var hit, dist, m_playerLayerMask) && hit.collider.gameObject == m_playerReference)
                       ? m_playerReference : null;
        if (PlayerTarget) { LastKnownPlayerPosition = PlayerTarget.transform.position; }

        // movement: chase if seen, otherwise stop (or go to last known if you prefer)
        if (agent && agent.isOnNavMesh)
        {
            if (PlayerTarget) agent.SetDestination(LastKnownPlayerPosition);
            else agent.ResetPath(); // or: agent.SetDestination(LastKnownPlayerPosition);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = PlayerTarget ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, m_detectionRange);
    }
}
