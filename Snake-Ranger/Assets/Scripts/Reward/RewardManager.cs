using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class RewardManager : MonoBehaviour
{
    [Header("References")]
    public PlayerSnake player;

    [Tooltip("All possible rewards (ScriptableObjects)")]
    public List<Reward> allRewards = new List<Reward>();

    [Header("UI")]
    public GameObject rewardPanel;           // The panel that holds the cards (enable/disable this)
    public Transform rewardContainer;        // Where to spawn 3 cards (a Horizontal/Vertical Layout)
    public GameObject rewardCardPrefab;      // Prefab with RewardCard script
    public int choicesCount = 3;
    public bool pauseOnChoice = true;

    private readonly List<GameObject> spawnedCards = new();

    void Awake()
    {
        // Subscribe to XPManager's level-up event
        if (XPManager.Instance != null)
            XPManager.Instance.OnLevelUp += ShowRewards;
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent leaks
        if (XPManager.Instance != null)
            XPManager.Instance.OnLevelUp -= ShowRewards;
    }

    public void ShowRewards(int newLevel)
    {
        if (pauseOnChoice) Time.timeScale = 0f;

        ClearCards();
        rewardPanel.SetActive(true);

        var picks = PickUniqueRewards(choicesCount);
        foreach (var r in picks)
        {
            var go = Instantiate(rewardCardPrefab, rewardContainer);
            spawnedCards.Add(go);
            var card = go.GetComponent<RewardCard>();
            card.Bind(r, OnRewardChosen);
        }
    }

    private void OnRewardChosen(Reward chosen)
    {
        chosen.Apply(player);

        // Close UI and cleanup
        ClearCards();
        rewardPanel.SetActive(false);
        if (pauseOnChoice) Time.timeScale = 1f;
    }

    private void ClearCards()
    {
        for (int i = spawnedCards.Count - 1; i >= 0; i--)
        {
            Destroy(spawnedCards[i]);
        }
        spawnedCards.Clear();
    }

    // --- Random selection helpers ---

    // Uniform unique selection
    private List<Reward> PickUniqueRewards(int count)
    {
        if (allRewards == null || allRewards.Count == 0)
            return new List<Reward>();

        var pool = new List<Reward>(allRewards);
        var result = new List<Reward>();
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            result.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        return result;
    }
}
