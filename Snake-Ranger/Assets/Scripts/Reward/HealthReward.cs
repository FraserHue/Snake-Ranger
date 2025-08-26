using UnityEngine;

[CreateAssetMenu(menuName = "Rewards/Health Reward", fileName = "HealthReward")]
public class HealthReward : Reward
{
    [SerializeField] private int bonusHealth = 20;

    public override void Apply(PlayerSnake player)
    {
        if (player == null)
        {
            Debug.LogError("HealthReward: Player is null!");
            return;
        }

        player.maxHealth += bonusHealth;

        // Optionally heal the player immediately
        player.currentHealth = Mathf.Min(player.currentHealth + bonusHealth, player.maxHealth);

        Debug.Log($"HealthReward applied: +{bonusHealth} max health.");
    }
}
