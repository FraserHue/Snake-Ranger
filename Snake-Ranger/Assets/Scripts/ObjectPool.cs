using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ObjectPool : MonoBehaviour
{
    public PoolableObject prefab;
    public int size = 50;
    private List<PoolableObject> available;

    void Awake()
    {
        available = new List<PoolableObject>(size);
        for (int i = 0; i < size; i++)
        {
            var p = Instantiate(prefab, transform);
            p.Parent = this;
            p.gameObject.SetActive(false);
            available.Add(p);
        }
    }

    public void ReturnObjectToPool(PoolableObject o)
    {
        var agent = o.GetComponent<NavMeshAgent>();
        if (agent != null) agent.ResetPath();
        o.gameObject.SetActive(false);
        if (!available.Contains(o)) available.Add(o);
    }

    public bool TryGetObject(out PoolableObject obj)
    {
        if (available.Count == 0)
        {
            obj = null;
            return false;
        }
        obj = available[0];
        available.RemoveAt(0);
        obj.gameObject.SetActive(true);
        return true;
    }
}