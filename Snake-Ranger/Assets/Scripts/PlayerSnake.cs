using UnityEngine;

public class PlayerSnake : MonoBehaviour
{

    [Header("Stats")]
    public float moveSpeed = 3f;
    public float damageMultiplier = 1f;
    public int maxHealth = 3;
    public int currentHealth = 3;

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }
}
