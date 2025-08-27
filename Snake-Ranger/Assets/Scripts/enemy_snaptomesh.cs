using UnityEngine;
using UnityEngine.AI;
public class SnapToNavMesh : MonoBehaviour
{
    // snap to the nearest navmesh at spawn so the agent is valid. all comments lower caps.
    void OnEnable()
    {
        var a = GetComponent<NavMeshAgent>();
        if (a && NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
            a.Warp(hit.position);
    }
}
