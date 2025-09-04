using System;
using System.Reflection;
using UnityEngine;
public class SetBehaviorTargetToNearestSegment : MonoBehaviour
{
    public string blackboardVariableName = "Target";
    public string snakeTag = "Player";
    // how often to update (seconds)
    public float updateInterval = 0.2f;

    float timer;

    void Start()
    {
        timer = UnityEngine.Random.Range(0f, updateInterval); 
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = updateInterval;

        var snakeRoot = FindSnakeRoot();
        if (snakeRoot == null) return;

        var nearest = FindNearestChildTransform(snakeRoot.transform, transform.position);
        if (nearest == null) return;

        TrySetBlackboardVariable(nearest.gameObject);
    }

    GameObject FindSnakeRoot()
    {
        var go = GameObject.FindWithTag(snakeTag);
        return go;
    }

    Transform FindNearestChildTransform(Transform root, Vector3 fromPos)
    {
        Transform best = null;
        float bestDist = float.MaxValue;

        var stack = new System.Collections.Generic.Stack<Transform>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var t = stack.Pop();
            if (t != root)
            {
                float d = (t.position - fromPos).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = t;
                }
            }
            for (int i = 0; i < t.childCount; i++) stack.Push(t.GetChild(i));
        }
        return best;
    }

    void TrySetBlackboardVariable(GameObject target)
    {
        var monos = GetComponents<MonoBehaviour>();
        foreach (var mb in monos)
        {
            if (mb == null) continue;
            var tname = mb.GetType().Name.ToLowerInvariant();
            if (tname.Contains("behavioragent") || tname.Contains("behaviouragent") || tname.Contains("behavior"))
            {
                var mi = mb.GetType().GetMethod("SetVariableValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (mi != null)
                {
                    try { mi.Invoke(mb, new object[] { blackboardVariableName, target }); return; } catch (Exception) { }
                }

                var prop = mb.GetType().GetProperty("BlackboardReference", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                object bbRef = prop != null ? prop.GetValue(mb) : null;
                if (bbRef == null)
                {
                    var field = mb.GetType().GetField("BlackboardReference", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    bbRef = field != null ? field.GetValue(mb) : null;
                }
                if (bbRef != null)
                {
                    var bbMi = bbRef.GetType().GetMethod("SetVariableValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (bbMi != null)
                    {
                        try { bbMi.Invoke(bbRef, new object[] { blackboardVariableName, target }); return; } catch (Exception) { }
                    }
                }
            }
        }
    }
}