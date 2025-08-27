using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RangeDetector", story: "Update Range [Detector] and assign [Player]", category: "Action", id: "e4179e8521c0f147ff53d9da5f30b80a")]
public partial class RangeDetectorAction : Action
{
    [SerializeReference] public BlackboardVariable<RangeDetector> Detector;
    [SerializeReference] public BlackboardVariable<GameObject> Player;

    protected override Status OnUpdate()
    {
        if (Detector?.Value == null) return Status.Failure;

        var found = Detector.Value.UpdateDetector();

        if (found != null)
            Player.Value = found;

        return found != null ? Status.Success : Status.Failure;
    }
}

