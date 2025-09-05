using UnityEngine;
using TMPro;

public class EnemySpawnCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textUi;

    void Awake()
    {
        if (textUi == null) textUi = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        // read counters from WaveSpawner
        int deaths = WaveSpawner.GlobalDeaths;
        int limit = WaveSpawner.GlobalLimit;

        if (limit > 0)
            textUi.text = $"Spiders: {deaths} / {limit}";
        else
            textUi.text = $"Spiders: ? / ?";
    }
}