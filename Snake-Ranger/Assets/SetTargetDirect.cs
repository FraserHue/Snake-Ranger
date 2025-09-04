using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class SetTargetDirect : MonoBehaviour
{
    public Component behaviorAgentComponent; // assign in inspector if you can
    public string variableName = "Target";

    // call this to set the blackboard target
    public void SetTarget(GameObject target)
    {
        if (behaviorAgentComponent == null || target == null) return;

        var type = behaviorAgentComponent.GetType();

        // 1) try direct, non-generic method with exact params: (string, object)
        var direct = type.GetMethod("SetVariableValue", new Type[] { typeof(string), typeof(object) });
        if (direct != null)
        {
            try
            {
                direct.Invoke(behaviorAgentComponent, new object[] { variableName, target });
                return;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"invoke direct SetVariableValue failed: {e.Message}");
            }
        }

        // 2) try to find a generic SetVariableValue<T>(string, T) and make it for GameObject
        var genericCandidates = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.Name == "SetVariableValue" && m.IsGenericMethodDefinition && m.GetParameters().Length == 2);

        foreach (var g in genericCandidates)
        {
            var pars = g.GetParameters();
            if (pars[0].ParameterType == typeof(string))
            {
                try
                {
                    var constructed = g.MakeGenericMethod(typeof(GameObject));
                    constructed.Invoke(behaviorAgentComponent, new object[] { variableName, target });
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"invoke generic SetVariableValue failed: {e.Message}");
                }
            }
        }

        // 3) fallback: try accessing BlackboardReference then calling its SetVariableValue
        object bbRef = null;
        var bbProp = type.GetProperty("BlackboardReference", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (bbProp != null) bbRef = bbProp.GetValue(behaviorAgentComponent);
        if (bbRef == null)
        {
            var bbField = type.GetField("BlackboardReference", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (bbField != null) bbRef = bbField.GetValue(behaviorAgentComponent);
        }

        if (bbRef != null)
        {
            var bbType = bbRef.GetType();
            // try direct first
            var bbDirect = bbType.GetMethod("SetVariableValue", new Type[] { typeof(string), typeof(object) });
            if (bbDirect != null)
            {
                try
                {
                    bbDirect.Invoke(bbRef, new object[] { variableName, target });
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"blackboard direct invoke failed: {e.Message}");
                }
            }

            // try generic on blackboard
            var bbGeneric = bbType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.Name == "SetVariableValue" && m.IsGenericMethodDefinition && m.GetParameters().Length == 2);
            foreach (var g in bbGeneric)
            {
                var pars = g.GetParameters();
                if (pars[0].ParameterType == typeof(string))
                {
                    try
                    {
                        var constructed = g.MakeGenericMethod(typeof(GameObject));
                        constructed.Invoke(bbRef, new object[] { variableName, target });
                        return;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"blackboard generic invoke failed: {e.Message}");
                    }
                }
            }
        }

        Debug.LogWarning($"could not set blackboard variable '{variableName}' on {type.Name} (ambiguous overloads may exist). assign the behaviour agent in inspector to avoid reflection.");
    }
}