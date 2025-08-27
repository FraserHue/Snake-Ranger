using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SetStoppingDistance", story: "[Self] Set Stopping to [AttackRange]", category: "Action", id: "e519c9bbad5f3053c6d4c97ea7118aac")]
public partial class SetStoppingDistanceAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<float> AttackRange;
    [SerializeField] float buffer = 0.3f;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self?.Value == null) return Status.Failure;
        var agent = Self.Value.GetComponent<NavMeshAgent>();
        if (!agent) return Status.Failure;

        agent.stoppingDistance = Mathf.Max(0f, AttackRange.Value - buffer);
        return Status.Success;
    }
}

