// DamagePopup.cs
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.8f;
    [SerializeField] private float riseSpeed = 1.0f;
    [SerializeField] private float horizontalJitter = 0.25f;
    [SerializeField] private float startScale = 1.0f;
    [SerializeField] private float endScale = 0.8f;

    static Transform s_prefab;

    private TextMeshPro _tmp;
    private float _time;
    private Color _baseColor;
    private Vector3 _dir;
    private Camera _cam;

    void Awake()
    {
        _tmp = GetComponent<TextMeshPro>();
        if (_tmp == null) _tmp = gameObject.AddComponent<TextMeshPro>();

        _baseColor = _tmp.color;
        _dir = new Vector3(Random.Range(-horizontalJitter, horizontalJitter), 0f, Random.Range(-horizontalJitter, horizontalJitter));
        _cam = Camera.main;
        transform.localScale = Vector3.one * startScale;
    }

    void LateUpdate()
    {
        _time += Time.deltaTime;

        // move up and drift
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;
        transform.position += _dir * 0.25f * Time.deltaTime;

        // face camera
        if (_cam == null) _cam = Camera.main;
        if (_cam != null)
        {
            transform.forward = (_cam.transform.position - transform.position).normalized * -1f;
        }

        // shrink
        float t = Mathf.Clamp01(_time / lifetime);
        transform.localScale = Vector3.Lerp(Vector3.one * startScale, Vector3.one * endScale, t);

        // fade
        float alpha = Mathf.Lerp(1f, 0f, t);
        _tmp.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);

        if (_time >= lifetime) Destroy(gameObject);
    }

    public void Setup(int damageAmount, bool isCritical = false)
    {
        // set text
        _tmp.SetText(damageAmount.ToString());

        // simple crit style
        if (isCritical)
        {
            _tmp.fontSize *= 1.2f;
        }
    }

    // set the prefab once at startup
    public static void SetPrefab(Transform prefab)
    {
        s_prefab = prefab;
    }

    // create at a world position (tutorial-style)
    public static DamagePopup Create(Vector3 worldPos, int damageAmount, bool isCritical = false)
    {
        if (s_prefab == null)
        {
            Debug.LogWarning("damagepopup prefab not set. call DamagePopup.SetPrefab(prefab) first.");
            return null;
        }

        Transform inst = Instantiate(s_prefab, worldPos, Quaternion.identity);
        var dp = inst.GetComponent<DamagePopup>();
        if (dp == null) dp = inst.gameObject.AddComponent<DamagePopup>();
        dp.Setup(damageAmount, isCritical);
        return dp;
    }

    // optional: world spawn with rotation if you ever need it
    public static DamagePopup Create(Vector3 worldPos, Quaternion rot, int damageAmount, bool isCritical = false)
    {
        if (s_prefab == null)
        {
            Debug.LogWarning("damagepopup prefab not set. call DamagePopup.SetPrefab(prefab) first.");
            return null;
        }

        Transform inst = Instantiate(s_prefab, worldPos, rot);
        var dp = inst.GetComponent<DamagePopup>();
        if (dp == null) dp = inst.gameObject.AddComponent<DamagePopup>();
        dp.Setup(damageAmount, isCritical);
        return dp;
    }
}
