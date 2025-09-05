using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.AI;

public class WaveSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private PoolableObject poolPrefab;
    [SerializeField] private bool usePooling = true;
    [SerializeField] private int poolSize = 50;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float minimumSpawnTime = 1f;
    [SerializeField] private float maximumSpawnTime = 3f;

    [Header("Global spawn (shared across all spawners)")]
    [SerializeField] private bool useGlobalLimit = true;
    [SerializeField] private int initialGlobalSpawnLimit = 10;

    [Header("Boss")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private bool spawnBossWhenLimitReached = true;
    [SerializeField] private TextMeshProUGUI bossSpawnText; 
    [SerializeField] private float bossAnnouncementDelay = 4f; 


    private static bool s_bossSpawned = false;

    private ObjectPool pool;
    private float timeUntilSpawn;

    private static int s_globalLimit = 0;
    private static int s_globalSpawned = 0;
    private static int s_globalDeaths = 0;

    public static int GlobalLimit => s_globalLimit;
    public static int GlobalSpawned => s_globalSpawned;
    public static int GlobalDeaths => s_globalDeaths;


    private int totalSpawned = 0;
    private int totalDeaths = 0;

    private static int s_deathListenerCount = 0;

    private void Awake()
    {
        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }
        SetTimeUntilSpawn();
    }

    private void OnEnable()
    {
        if (useGlobalLimit)
        {
            s_deathListenerCount++;
            if (s_deathListenerCount == 1)
                Enemy.OnAnyEnemyDied += OnEnemyDied;
        }
    }

    private void OnDisable()
    {
        if (useGlobalLimit)
        {
            s_deathListenerCount = Mathf.Max(0, s_deathListenerCount - 1);
            if (s_deathListenerCount == 0)
                Enemy.OnAnyEnemyDied -= OnEnemyDied;
        }
    }

    private void Start()
    {
        if (useGlobalLimit && s_globalLimit == 0)
        {
            var cfg = FindObjectOfType<SpawnConfig>();
            if (cfg != null)
                s_globalLimit = Mathf.Max(0, cfg.globalSpawnLimit);
            else
                s_globalLimit = Mathf.Max(0, initialGlobalSpawnLimit);
        }
        if (usePooling)
        {
            pool = FindObjectOfType<ObjectPool>();
            if (pool == null && poolPrefab != null)
            {
                var poolGO = new GameObject("Pooling");
                pool = poolGO.AddComponent<ObjectPool>();
                pool.prefab = poolPrefab;
                pool.size = poolSize;
            }
        }
    }

    private void Update()
    {
        timeUntilSpawn -= Time.deltaTime;

        if (timeUntilSpawn <= 0f)
        {
            SpawnOne();
            SetTimeUntilSpawn();
        }
    }

    private bool ShouldStopSpawning()
    {
        if (s_bossSpawned) return true;

        if (useGlobalLimit && s_globalLimit > 0)
            return s_globalDeaths >= s_globalLimit || s_globalSpawned >= s_globalLimit;

        if (initialGlobalSpawnLimit <= 0) return false;
        return totalDeaths >= initialGlobalSpawnLimit || totalSpawned >= initialGlobalSpawnLimit;
    }

    private void SpawnOne()
    {
        if (ShouldStopSpawning()) return;

        Transform sp = (spawnPoints != null && spawnPoints.Length > 0)
            ? spawnPoints[Random.Range(0, spawnPoints.Length)]
            : transform;

        if (usePooling && pool != null)
        {
            PoolableObject pooled;
            if (pool.TryGetObject(out pooled))
            {
                pooled.transform.position = sp.position;
                pooled.transform.rotation = sp.rotation;
                var ai = pooled.GetComponent<EnemyAI>();
                if (ai != null) ai.SetTarget(player);
                var enemyComp = pooled.GetComponent<Enemy>();
                if (enemyComp != null) enemyComp.ResetState();

                if (useGlobalLimit && s_globalLimit > 0) s_globalSpawned++;
                else totalSpawned++;

                Debug.Log($"spawned (pooled). globalSpawned={s_globalSpawned} globalDeaths={s_globalDeaths}");
            }
            else
            {
                Debug.Log($"pool empty -> available={pool.AvailableCount} pool.size={pool.size}");
            }
        }
        else
        {
            var go = Instantiate(enemyPrefab, sp.position, sp.rotation);
            var ai = go.GetComponent<EnemyAI>();
            if (ai != null) ai.SetTarget(player);

            if (useGlobalLimit && s_globalLimit > 0) s_globalSpawned++;
            else totalSpawned++;

            Debug.Log($"spawned (inst). globalSpawned={s_globalSpawned} globalDeaths={s_globalDeaths}");
        }
    }

    private void OnEnemyDied(Enemy e)
    {
        if (s_bossSpawned) return;

        if (useGlobalLimit && s_globalLimit > 0) s_globalDeaths++;
        else totalDeaths++;

        Debug.Log($"enemy died. globalSpawned={s_globalSpawned} globalDeaths={s_globalDeaths}");

        if (spawnBossWhenLimitReached && !s_bossSpawned && (useGlobalLimit ? s_globalDeaths >= s_globalLimit : totalDeaths >= initialGlobalSpawnLimit))
        {
            TrySpawnBoss();
        }
    }

    private void TrySpawnBoss()
    {
        if (s_bossSpawned) return;
        if (bossPrefab == null)
        {
            Debug.LogWarning("boss prefab not set on WaveSpawner.");
            return;
        }
        StartCoroutine(SpawnBossWithAnnouncement());
    }

    private IEnumerator SpawnBossWithAnnouncement()
    {
        s_bossSpawned = true;

        if (bossSpawnText != null)
        {
            bossSpawnText.gameObject.SetActive(true);
            StartCoroutine(AnimateBossText(bossSpawnText, bossAnnouncementDelay + 2f));
        }

        yield return new WaitForSeconds(bossAnnouncementDelay);

        Vector3 pos = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
        Quaternion rot = bossSpawnPoint != null ? bossSpawnPoint.rotation : transform.rotation;

        GameObject bossGO = Instantiate(bossPrefab, pos, rot);

        EnemyAI bossAI = bossGO.GetComponent<EnemyAI>() ?? bossGO.GetComponentInChildren<EnemyAI>();
        if (bossAI != null)
        {
            bossAI.SetTarget(player);
        }
        else
        {
            Debug.LogWarning("Spawned boss has no EnemyAI component (on root or child).");
        }

        NavMeshAgent bossAgent = bossGO.GetComponent<NavMeshAgent>() ?? bossGO.GetComponentInChildren<NavMeshAgent>();
        if (bossAgent != null)
        {
            bossAgent.Warp(pos);
            bossAgent.isStopped = false;
        }
        else
        {
            Debug.LogWarning("Spawned boss has no NavMeshAgent (on root or child).");
        }
    }

    private IEnumerator AnimateBossText(TextMeshProUGUI txt, float totalTime)
    {
        if (txt == null) yield break;
        float fadeIn = 0.45f;
        float fadeOut = 0.45f;
        float elapsed = 0f;

        txt.transform.localScale = Vector3.one * 0.9f;
        Color c = txt.color; c.a = 0f; txt.color = c;

        while (elapsed < fadeIn)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / fadeIn);
            c.a = Mathf.Lerp(0f, 1f, t);
            txt.color = c;
            txt.transform.localScale = Vector3.one * Mathf.Lerp(0.9f, 1.08f, t);
            yield return null;
        }

        float remaining = Mathf.Max(0f, totalTime - fadeIn - fadeOut);
        float pulseTimer = 0f;
        while (remaining > 0f)
        {
            pulseTimer += Time.deltaTime;
            remaining -= Time.deltaTime;
            float pulse = 1f + Mathf.Sin(pulseTimer * 3.0f) * 0.04f; 
            txt.transform.localScale = Vector3.one * pulse;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < fadeOut)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOut;
            c.a = Mathf.Lerp(1f, 0f, t);
            txt.color = c;
            txt.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.95f, t);
            yield return null;
        }

        txt.gameObject.SetActive(false);
    }

    private void SetTimeUntilSpawn()
    {
        timeUntilSpawn = Random.Range(minimumSpawnTime, maximumSpawnTime);
    }

    public static void ResetGlobalCounters()
    {
        s_globalLimit = 0;
        s_globalSpawned = 0;
        s_globalDeaths = 0;
        s_bossSpawned = false;
    }
}