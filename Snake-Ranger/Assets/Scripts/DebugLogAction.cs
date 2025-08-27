using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DebugLog", story: "log [Message]", category: "Action", id: "e7b307a4407bbaec4acacc1e3184fdfc")]
public partial class DebugLogAction : Action
{
    [SerializeReference] public BlackboardVariable<string> Message;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Debug.Log($"[BehaviorGraph] {Message.Value}");
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

