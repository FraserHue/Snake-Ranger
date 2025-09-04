using UnityEngine;
using System.Collections.Generic;

public class SnakeController : MonoBehaviour
{
    public float MoveSpeed = 5f;
    public float SteerSpeed = 300f;
    public int BodySpeed = 5;
    public int Gap = 10;
    public int InitialSnakeLength = 10;

    public GameObject BodyPrefab;

    private readonly List<GameObject> BodyParts = new List<GameObject>();
    private readonly List<Vector3> PositionHistory = new List<Vector3>();

    public float AimSmoothTime = 0.06f;
    private Vector3 aimPoint;
    private Vector3 aimVel;

    private bool isMoving = true;

    public bool UseMouseControl = true;
    public bool UseADControl = true;

    public float LungeSpeed = 25f;
    public float LungeDuration = 0.2f;
    public float InvincibilityDuration = 0.5f;

    private float lungeTimeRemaining = 0f;
    private bool isInvincible = false;
    public bool IsInvincible => isInvincible;

    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float collisionCheckDistance = 0.2f;
    [SerializeField] private float pushBackDistance = 0.5f;
    [SerializeField] private float headRadius = 0.25f;
    [SerializeField] private int collisionDamage = 1;
    [SerializeField] private float damageCooldown = 0.15f;

    private float _lastDamageTime = -999f;
    private SnakeStatus _status;

    void Start()
    {
        int saved = PlayerPrefs.GetInt("UseKeyboardControl", 1);
        bool keyboardEnabled = saved == 1;
        ApplyKeyboardSetting(keyboardEnabled);

        for (int i = 0; i < InitialSnakeLength; i++) GrowSnake();
        aimPoint = transform.position;
        _status = GetComponent<SnakeStatus>();
        if (obstacleMask == 0) obstacleMask = LayerMask.GetMask("Obstacles");
    }

    public void SetUseKeyboardControl(bool enabled)
    {
        PlayerPrefs.SetInt("UseKeyboardControl", enabled ? 1 : 0);
        PlayerPrefs.Save();
        ApplyKeyboardSetting(enabled);
    }

    private void ApplyKeyboardSetting(bool keyboardEnabled)
    {
        UseADControl = keyboardEnabled;
        UseMouseControl = !keyboardEnabled;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) isMoving = false;
        if (Input.GetKeyDown(KeyCode.W)) isMoving = true;

        if (Input.GetMouseButtonDown(1) && _status != null && _status.TryConsumeLunge())
        {
            TriggerLunge();
        }

        bool lungeActive = lungeTimeRemaining > 0f;
        Vector3 totalMove = Vector3.zero;

        if (lungeActive)
        {
            totalMove += transform.forward * LungeSpeed * Time.deltaTime;
            lungeTimeRemaining -= Time.deltaTime;
            if (lungeTimeRemaining < 0f) lungeTimeRemaining = 0f;
        }

        if (isMoving)
        {
            totalMove += transform.forward * MoveSpeed * Time.deltaTime;
        }

        if ((isMoving || lungeActive) && totalMove.sqrMagnitude > 0f)
        {
            Vector3 start = transform.position;
            Vector3 dir = totalMove.normalized;
            float dist = totalMove.magnitude;

            bool hitSomething = false;
            Vector3 hp = Vector3.zero;
            Vector3 hn = Vector3.zero;
            RaycastHit hit;

            if (Physics.SphereCast(start, headRadius, dir, out hit, dist, obstacleMask, QueryTriggerInteraction.Collide))
            {
                hitSomething = true; hp = hit.point; hn = hit.normal;
            }
            else if (Physics.Raycast(start, transform.forward, out hit, Mathf.Max(collisionCheckDistance, dist), obstacleMask, QueryTriggerInteraction.Collide))
            {
                hitSomething = true; hp = hit.point; hn = hit.normal;
            }
            else
            {
                Vector3 end = start + totalMove;
                Collider[] cols = Physics.OverlapSphere(end, headRadius, obstacleMask, QueryTriggerInteraction.Collide);
                if (cols.Length > 0)
                {
                    float best = -1f;
                    for (int i = 0; i < cols.Length; i++)
                    {
                        Vector3 p = cols[i].ClosestPoint(end);
                        Vector3 n = end - p;
                        float s = n.sqrMagnitude;
                        if (s > best)
                        {
                            best = s;
                            hp = p;
                            hn = n.sqrMagnitude > 1e-6f ? n.normalized : -transform.forward;
                            hitSomething = true;
                        }
                    }
                }
            }

            if (hitSomething)
            {
                if (!IsInvincible && (Time.time - _lastDamageTime) >= damageCooldown)
                {
                    if (_status != null) _status.TakeDamage(collisionDamage);
                    _lastDamageTime = Time.time;
                }

                Vector3 reflectDir = Vector3.Reflect(transform.forward, hn);
                reflectDir.y = 0f;
                if (reflectDir.sqrMagnitude < 1e-6f) reflectDir = Vector3.Cross(Vector3.up, hn).normalized;

                transform.position = hp + hn * Mathf.Max(pushBackDistance, headRadius * 1.1f);
                transform.rotation = Quaternion.LookRotation(reflectDir.normalized, Vector3.up);
                transform.position += reflectDir.normalized * 0.01f;
                totalMove = Vector3.zero;
            }
        }

        if (totalMove.sqrMagnitude > 0f)
        {
            transform.position += totalMove;
        }

        if (isMoving || lungeActive)
        {
            if (UseMouseControl)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
                float enter;
                if (groundPlane.Raycast(ray, out enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    aimPoint = Vector3.SmoothDamp(aimPoint, hitPoint, ref aimVel, AimSmoothTime);
                    Vector3 toMouse = aimPoint - transform.position;
                    toMouse.y = 0f;
                    if (toMouse.sqrMagnitude > 0.0001f)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(toMouse.normalized, Vector3.up);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, SteerSpeed * Time.deltaTime);
                    }
                }
            }

            if (UseADControl)
            {
                float steerDirection = Input.GetAxis("Horizontal");
                if (Mathf.Abs(steerDirection) > 0.0001f)
                    transform.Rotate(Vector3.up * steerDirection * SteerSpeed * Time.deltaTime);
            }
        }

        if (isMoving || lungeActive)
        {
            PositionHistory.Insert(0, transform.position);
        }

        int needed = (BodyParts.Count + 1) * Gap;
        if (PositionHistory.Count > needed)
        {
            PositionHistory.RemoveAt(PositionHistory.Count - 1);
        }

        if (isMoving || lungeActive)
        {
            int index = 1;
            foreach (var body in BodyParts)
            {
                Vector3 point = PositionHistory[Mathf.Clamp(index * Gap, 0, PositionHistory.Count - 1)];
                Vector3 moveDirection = point - body.transform.position;
                body.transform.position += moveDirection * BodySpeed * Time.deltaTime;
                body.transform.LookAt(point);
                index++;
            }
        }
    }

    private void GrowSnake()
    {
        GameObject body = Instantiate(BodyPrefab);
        BodyParts.Add(body);
    }

    public void TriggerLunge()
    {
        lungeTimeRemaining = Mathf.Max(lungeTimeRemaining, LungeDuration);
        if (!isInvincible) StartCoroutine(InvincibilityWindow(InvincibilityDuration));
    }

    System.Collections.IEnumerator InvincibilityWindow(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }
}
