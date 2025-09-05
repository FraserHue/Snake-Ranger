using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public Transform target;
    public float followUpdateInterval = 0.2f;
    public float targetHeightOffset = 0.5f; 

    NavMeshAgent agent;
    Coroutine followRoutine;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void SetTarget(Transform t)
    {
        if (t == null) return;
        target = t.root;
        if (followRoutine != null) StopCoroutine(followRoutine);
        followRoutine = StartCoroutine(FollowTargetRoutine());
    }

    IEnumerator FollowTargetRoutine()
    {
        while (target != null)
        {
            Vector3 aim = target.position;
            aim.y += targetHeightOffset;
            agent.SetDestination(aim);
            yield return new WaitForSeconds(followUpdateInterval);
        }
    }

    void OnDisable()
    {
        if (followRoutine != null) StopCoroutine(followRoutine);
    }
}