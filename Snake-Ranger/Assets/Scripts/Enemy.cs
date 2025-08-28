using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int maxHealth = 30;
    [SerializeField] private int currentHealth = 30;
    [SerializeField] private float despawnDelay = 0f; // 0 = instant

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead { get; private set; }

    void Awake()
    {
        // Clamp starting values
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        IsDead = currentHealth <= 0;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        if (amount <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        Debug.Log($"Enemy '{name}' took {amount} damage ({currentHealth}/{maxHealth})", this);

        if (currentHealth == 0)
            Die();
    }

    public void Kill()
    {
        if (IsDead) return;
        currentHealth = 0;
        Die();
    }

    void Die()
    {
        if (IsDead) return;
        IsDead = true;
        Debug.Log($"Enemy '{name}' died", this);
        Destroy(gameObject, despawnDelay); // remove from scene
    }
}
