using UnityEngine;
using System;

public class SnakeStatus : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int currentHealth = 10;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int length = 10;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = Mathf.Max(0f, value); }
    public int Length { get => length; set => length = Mathf.Max(1, value); }
    public bool IsDead { get; private set; }

    public event Action<int,int> OnHealthChanged;
    public event Action OnDied;
    public event Action<int> OnDamaged;
    public event Action<int> OnHealed;

    SnakeController controller;

    void Awake()
    {
        controller = GetComponent<SnakeController>();
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        IsDead = currentHealth <= 0;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        if (amount <= 0) return;
        if (controller != null && controller.IsInvincible) return;

        int prev = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnDamaged?.Invoke(amount);
        if (currentHealth != prev) OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth == 0)
        {
            IsDead = true;
            OnDied?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        if (amount <= 0) return;

        int prev = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealed?.Invoke(amount);
        if (currentHealth != prev) OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetMaxHealth(int newMax, bool clampCurrent = true)
    {
        maxHealth = Mathf.Max(1, newMax);
        if (clampCurrent) currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Revive(int newHealth)
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(newHealth, 1, maxHealth);
        IsDead = false;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
