using UnityEngine;
using UnityEngine.AI;
public class DebugAgent : MonoBehaviour
{   
    // obsolete
    // snaps to nearest navmesh, chases player, logs state. all comments lower caps.
    public Transform target;
    NavMeshAgent a;
    void Awake()
    {
        a = GetComponent<NavMeshAgent>();
        if (!target) { var p = GameObject.FindGameObjectWithTag("Player"); if (p) target = p.transform; }
    }
    void Start()
    {
        if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas)) a.Warp(hit.position);
        Debug.Log($"onNavMesh={a.isOnNavMesh}, typeID={a.agentTypeID}");
    }
    void Update()
    {
        if (!a || !a.isOnNavMesh) { Debug.LogWarning("agent not on navmesh"); return; }
        if (target) { a.SetDestination(target.position); Debug.DrawLine(transform.position, a.destination, Color.green); }
        if (a.pathPending || a.pathStatus != NavMeshPathStatus.PathComplete)
            Debug.Log($"pending={a.pathPending}, status={a.pathStatus}");
    }
}