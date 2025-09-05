using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int maxHealth = 30;
    [SerializeField] private int currentHealth = 30;
    [SerializeField] private float despawnDelay = 0f;

    public static event Action<Enemy> OnAnyEnemyDied;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead { get; private set; }

    void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        IsDead = currentHealth <= 0;
        if (IsDead) Die();
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        if (amount <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (currentHealth <= 0)
            Die();
    }

    public void ResetState()
    {
        currentHealth = maxHealth;
        IsDead = false;
        foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = true;
        gameObject.layer = 0;
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.ResetPath();
        gameObject.SetActive(true);
    }

    void Die()
    {
        if (IsDead) return;
        IsDead = true;

        OnAnyEnemyDied?.Invoke(this);

        if (GetComponent<Boss>() != null)
        {
            var win = FindObjectOfType<WinScreenManager>();
            if (win != null)
            {
                float delay = Mathf.Max(0.5f, despawnDelay);
                win.ShowWinDelayed(delay);
            }
        }

        foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = false;
        gameObject.layer = 2;

        var poolable = GetComponent<PoolableObject>();
        if (poolable != null && poolable.Parent != null)
        {
            poolable.Parent.ReturnObjectToPool(poolable);
            return;
        }

        if (despawnDelay <= 0f)
            Destroy(transform.root.gameObject);
        else
            StartCoroutine(DespawnAfter(despawnDelay));
    }

    System.Collections.IEnumerator DespawnAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (this) Destroy(transform.root.gameObject);
    }
}
