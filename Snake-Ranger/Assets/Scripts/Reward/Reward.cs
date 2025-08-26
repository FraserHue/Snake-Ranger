using UnityEngine;

public abstract class Reward : ScriptableObject
{
    public string rewardName;
    [TextArea] public string description;
    public Sprite icon;

    public abstract void Apply(PlayerSnake player);
}
