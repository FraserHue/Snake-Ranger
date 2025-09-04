using UnityEngine;
using System.Collections;

public class Boss : MonoBehaviour
{
    [SerializeField] Transform projectilePrefab;
    [SerializeField] Transform altProjectilePrefab;
    [SerializeField] float webSpeed = 6f;
    [SerializeField] float burstInterval = 10f;
    [SerializeField] float projectileLifetime = 6f;

    [SerializeField] int shotsPerBurst = 4;
    [SerializeField] float perShotDelay = 0.1f;

    static readonly Vector3[] WEB_DIRECTIONS = new Vector3[]
    {
        Vector3.right,
        Vector3.left,
        Vector3.forward,
        Vector3.back,
        (Vector3.right + Vector3.forward),
        (Vector3.right + Vector3.back),
        (Vector3.left + Vector3.forward),
        (Vector3.left + Vector3.back)
    };

    [SerializeField] float tongueInterval = 5f;

    Coroutine _webLoop;

    void OnEnable()
    {
        if (_webLoop == null) _webLoop = StartCoroutine(WebBurstLoop());
    }

    void OnDisable()
    {
        if (_webLoop != null) StopCoroutine(_webLoop);
        _webLoop = null;
    }

    IEnumerator WebBurstLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(burstInterval);
            yield return StartCoroutine(FireWebBurst());
        }
    }

    IEnumerator FireWebBurst()
    {
        if (projectilePrefab == null) yield break;

        int shots = Mathf.Max(1, shotsPerBurst);
        int[] specialShotIndex = new int[WEB_DIRECTIONS.Length];
        for (int d = 0; d < WEB_DIRECTIONS.Length; d++)
            specialShotIndex[d] = Random.Range(0, shots); // exactly one per direction

        for (int shot = 0; shot < shots; shot++)
        {
            Vector3 origin = transform.position;

            for (int d = 0; d < WEB_DIRECTIONS.Length; d++)
            {
                bool useAlt = (shot == specialShotIndex[d]) && altProjectilePrefab != null;
                Transform prefab = useAlt ? altProjectilePrefab : projectilePrefab;
                SpawnProjectile(origin, WEB_DIRECTIONS[d].normalized, prefab);
            }

            if (shot < shots - 1)
                yield return new WaitForSeconds(perShotDelay);
        }
    }

    void SpawnProjectile(Vector3 position, Vector3 direction, Transform prefab)
    {
        Transform proj = Instantiate(prefab, position, Quaternion.identity);

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * webSpeed;
        }
        else
        {
            var mover = proj.gameObject.AddComponent<SimpleLinearMover>();
            mover.direction = direction;
            mover.speed = webSpeed;
        }

        if (projectileLifetime > 0f)
            Destroy(proj.gameObject, projectileLifetime);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Vector3 p = transform.position;
        float rayLen = 1.25f;
        for (int i = 0; i < WEB_DIRECTIONS.Length; i++)
            Gizmos.DrawLine(p, p + WEB_DIRECTIONS[i].normalized * rayLen);
    }

    private class SimpleLinearMover : MonoBehaviour
    {
        [HideInInspector] public Vector3 direction = Vector3.forward;
        [HideInInspector] public float speed = 6f;

        void Update()
        {
            transform.position += direction * speed * Time.deltaTime;
        }
    }
}
