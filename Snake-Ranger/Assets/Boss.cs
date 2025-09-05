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

    [SerializeField] Transform tonguePrefab;
    [SerializeField] float tongueInterval = 5f;
    [SerializeField] float tongueDuration = 0.6f;
    [SerializeField] float tongueExtraOffset = 0.02f;

    [SerializeField] float projectileYOffset = 0f;
    [SerializeField] float tongueYOffset = 0f;

    [SerializeField] Animator animator;

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

    Coroutine _webLoop, _tongueLoop;

    void OnEnable()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (_webLoop == null) _webLoop = StartCoroutine(WebBurstLoop());
        if (_tongueLoop == null) _tongueLoop = StartCoroutine(TongueLoop());
    }

    void OnDisable()
    {
        if (_webLoop != null) StopCoroutine(_webLoop);
        if (_tongueLoop != null) StopCoroutine(_tongueLoop);
        _webLoop = null; _tongueLoop = null;
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
            specialShotIndex[d] = Random.Range(0, shots);

        for (int shot = 0; shot < shots; shot++)
        {
            Vector3 origin = transform.position + Vector3.up * projectileYOffset;

            for (int d = 0; d < WEB_DIRECTIONS.Length; d++)
            {
                bool useAlt = (shot == specialShotIndex[d]) && altProjectilePrefab != null;
                Transform prefab = useAlt ? altProjectilePrefab : projectilePrefab;
                SpawnProjectile(origin, WEB_DIRECTIONS[d].normalized, prefab);
            }

            if (shot < shots - 1) yield return new WaitForSeconds(perShotDelay);
        }
    }

    IEnumerator TongueLoop()
    {
        if (tonguePrefab == null) yield break;

        while (true)
        {
            yield return new WaitForSeconds(tongueInterval);

            if (animator) animator.SetTrigger("Tongue");

            float surface = ForwardSurfaceDistance();
            Vector3 worldPos = transform.position + Vector3.up * tongueYOffset + transform.forward * (surface + tongueExtraOffset);
            Transform t = Instantiate(tonguePrefab, worldPos, tonguePrefab.rotation);
            t.SetParent(transform, true);

            if (tongueDuration > 0f) Destroy(t.gameObject, tongueDuration);
        }
    }

    float ForwardSurfaceDistance()
    {
        Collider c = GetComponent<Collider>();
        if (c != null)
        {
            Vector3 e = c.bounds.extents;
            Vector3 f = transform.forward;
            f = new Vector3(Mathf.Abs(f.x), Mathf.Abs(f.y), Mathf.Abs(f.z));
            return e.x * f.x + e.y * f.y + e.z * f.z;
        }

        Renderer r = GetComponent<Renderer>();
        if (r != null)
        {
            Vector3 e = r.bounds.extents;
            Vector3 f = transform.forward;
            f = new Vector3(Mathf.Abs(f.x), Mathf.Abs(f.y), Mathf.Abs(f.z));
            return e.x * f.x + e.y * f.y + e.z * f.z;
        }

        return 0.5f;
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

        if (projectileLifetime > 0f) Destroy(proj.gameObject, projectileLifetime);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Vector3 p = transform.position + Vector3.up * projectileYOffset;
        float rayLen = 1.25f;
        for (int i = 0; i < WEB_DIRECTIONS.Length; i++)
            Gizmos.DrawLine(p, p + WEB_DIRECTIONS[i].normalized * rayLen);

        Vector3 t0 = transform.position + Vector3.up * tongueYOffset;
        Gizmos.DrawLine(t0, t0 + transform.forward * 1.25f);
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
