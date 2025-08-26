using UnityEngine;

public class DebugXP : MonoBehaviour
{
    [Header("Debug Settings")]
    public int xpToAdd = 999;      // Amount of XP to add when key is pressed
    public KeyCode debugKey = KeyCode.L; // Key to trigger level up

    void Update()
    {
        if (Input.GetKeyDown(debugKey))
        {
            if (XPManager.Instance != null)
            {
                XPManager.Instance.AddXP(xpToAdd);
                Debug.Log($"Added {xpToAdd} XP for debugging.");
            }
            else
            {
                Debug.LogWarning("XPManager.Instance is null!");
            }
        }
    }
}
