using UnityEngine;

[CreateAssetMenu(menuName = "Rewards/Damage Reward", fileName = "DamageReward")]
public class DamageReward : Reward
{
    [SerializeField] private float percent = 0.2f;

    public override void Apply(PlayerSnake player)
    {
        player.damageMultiplier *= (1f + percent);
    }
}
