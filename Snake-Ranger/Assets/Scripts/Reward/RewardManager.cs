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
    public GameObject rewardPanel;           // The panel that holds the cards
    public GameObject rewardCardPrefab;      // Prefab with RewardCard script
    public int choicesCount = 3;
    public bool pauseOnChoice = true;

    private readonly List<GameObject> spawnedCards = new List<GameObject>();

    private Queue<int> pendingLevels = new Queue<int>();
    private bool showingRewards = false;

    [Header("Audio")]
    AudioManager audioManager;

    private void Awake()
    {
        var audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null) audioManager = audioObj.GetComponent<AudioManager>();
    }

    void Start()
    {
        if (XPManager.Instance != null)
            XPManager.Instance.OnLevelUp += QueueReward;
    }

    void OnDestroy()
    {
        if (XPManager.Instance != null)
            XPManager.Instance.OnLevelUp -= QueueReward;
    }

    private void QueueReward(int newLevel)
    {
        pendingLevels.Enqueue(newLevel);
        if (!showingRewards)
        {
            audioManager.PlaySFX(audioManager.levelUp);
            ShowNextReward();

        }
    }

    private void ShowNextReward()
    {
        if (pendingLevels.Count == 0)
        {
            showingRewards = false;
            return;
        }

        showingRewards = true;
        int lvl = pendingLevels.Dequeue();
        Debug.Log($"Showing reward for level {lvl}");

        if (pauseOnChoice) Time.timeScale = 0f;
        ClearCards();
        rewardPanel.SetActive(true);

        List<Reward> picks = PickUniqueRewards(choicesCount);
        foreach (Reward r in picks)
        {
            GameObject go = Instantiate(rewardCardPrefab, rewardPanel.transform);
            spawnedCards.Add(go);
            RewardCard card = go.GetComponent<RewardCard>();
            card.Bind(r, OnRewardChosen);
        }
    }

    private void OnRewardChosen(Reward chosen)
    {
        chosen.Apply(player);

        ClearCards();
        rewardPanel.SetActive(false);
        if (pauseOnChoice) Time.timeScale = 1f;

        // After player picks, show next queued reward
        ShowNextReward();
    }
    private void ClearCards()
    {
        for (int i = spawnedCards.Count - 1; i >= 0; i--)
        {
            Destroy(spawnedCards[i]);
        }
        spawnedCards.Clear();
    }

    // Uniform unique selection
    private List<Reward> PickUniqueRewards(int count)
    {
        if (allRewards == null || allRewards.Count == 0)
            return new List<Reward>();

        List<Reward> pool = new List<Reward>(allRewards);
        List<Reward> result = new List<Reward>();
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            result.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        return result;
    }
}
