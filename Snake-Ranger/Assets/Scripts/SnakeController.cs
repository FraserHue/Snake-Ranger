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

    // Lunge + i-frames
    public float LungeSpeed = 25f;
    public float LungeDuration = 0.2f;
    public float InvincibilityDuration = 0.5f;

    private float lungeTimeRemaining = 0f;
    private bool isInvincible = false;

    public bool IsInvincible => isInvincible;

    void Start()
    {
        for (int i = 0; i < InitialSnakeLength; i++) GrowSnake();
        aimPoint = transform.position;
    }
    

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) isMoving = false;
        if (Input.GetKeyDown(KeyCode.W)) isMoving = true;

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
