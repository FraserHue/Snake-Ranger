using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private PoolableObject poolPrefab;
    [SerializeField] private bool usePooling = true;
    [SerializeField] private int poolSize = 50;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float minimumSpawnTime = 1f;
    [SerializeField] private float maximumSpawnTime = 3f;

    private ObjectPool pool;
    private float timeUntilSpawn;

    private void Awake()
    {
        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }
        SetTimeUntilSpawn();
    }

    private void Start()
    {
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

        if (timeUntilSpawn <= 0)
        {
            SpawnOne();
            SetTimeUntilSpawn();
        }
    }

    private void SpawnOne()
    {
        Transform sp = (spawnPoints != null && spawnPoints.Length > 0) ? spawnPoints[Random.Range(0, spawnPoints.Length)] : transform;

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
            }
        }
        else
        {
            var go = Instantiate(enemyPrefab, sp.position, sp.rotation);
            var ai = go.GetComponent<EnemyAI>();
            if (ai != null) ai.SetTarget(player);
        }
    }

    private void SetTimeUntilSpawn()
    {
        timeUntilSpawn = Random.Range(minimumSpawnTime, maximumSpawnTime);
    }
}