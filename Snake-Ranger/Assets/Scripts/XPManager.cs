using UnityEngine;

public class XPManager : MonoBehaviour
{
    public static XPManager Instance;

    public delegate void XPChangeHandler(int currentXP, int requiredXP, int level);
    public event XPChangeHandler OnXPChanged;

    public delegate void LevelUpHandler(int newLevel);
    public event LevelUpHandler OnLevelUp;

    [Header("XP Settings")]
    public int currentXP = 0;
    public int requiredXP = 10; 
    public int level = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Add XP and check for level ups
    public void AddXP(int amount)
    {
        currentXP += amount;

        // Notify UI
        OnXPChanged?.Invoke(currentXP, requiredXP, level);

        // Level up loop
        while (currentXP >= requiredXP)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentXP -= requiredXP;
        level++;

        // Increase required XP for next level (you can tweak scaling)
        requiredXP = Mathf.RoundToInt(requiredXP * 1.5f);

        Debug.Log("Level Up! New level: " + level);

        // Fire event so RewardManager can listen
        OnLevelUp?.Invoke(level);

        // Notify XP bar again after level up
        OnXPChanged?.Invoke(currentXP, requiredXP, level);
    }

}
