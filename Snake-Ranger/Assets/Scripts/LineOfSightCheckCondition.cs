using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Line of Sight check", story: "Check [Player] with Line Of Sight [detector]", category: "Conditions", id: "f8528968d3ffe9eb284a515ceded5b40")]
public partial class LineOfSightCheckCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<LineOfSightDetector> Detector;

    public override bool IsTrue()
    {
        return Detector.Value.PerformDetection(Player.Value) != null;
    }

}
