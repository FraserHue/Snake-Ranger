using UnityEngine;
using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{

    [Header("References")]
    public PlayerSnake playerStats;
    public LayerMask obstacleMask;

    public float SteerSpeed = 300f;
    public int Gap = 10;
    public int InitialSnakeLength = 10;
    public float collisionCheckDistance = 0.05f;
    public float pushBackDistance = 0.5f;


    public GameObject BodyPrefab;

    private List<GameObject> BodyParts = new List<GameObject>();
    private List<Vector3> PositionHistory = new List<Vector3>();

    public float AimSmoothTime = 0.06f;
    private Vector3 aimPoint;
    private Vector3 aimVel;

    private bool isMoving = true;

    public bool UseMouseControl = true;
    public bool UseADControl = true;


    void Start()
    {
        for (int i = 0; i < InitialSnakeLength; i++) GrowSnake();
        aimPoint = transform.position;

        if (playerStats == null)
            playerStats = Object.FindFirstObjectByType<PlayerSnake>();
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.S)) isMoving = false;
        if (Input.GetKeyDown(KeyCode.W)) isMoving = true;

        // forward movement
        if (isMoving && playerStats != null)
        {
            Vector3 moveDir = transform.forward;
            Vector3 nextPos = transform.position + moveDir * playerStats.moveSpeed * Time.deltaTime;

            // Check if wall ahead
            if (Physics.Raycast(transform.position, moveDir, out RaycastHit hit, collisionCheckDistance, obstacleMask))
            {
                // Damage on hit
                playerStats.currentHealth -= 1;
                Debug.Log("Snake hit obstacle! Health: " + playerStats.currentHealth);

                // Reflect movement direction (bounce)
                Vector3 reflectDir = Vector3.Reflect(moveDir, hit.normal);
                reflectDir.y = 0f;

                // Push slightly away so it doesn't stick
                transform.position = hit.point + hit.normal * pushBackDistance;

                // Rotate snake to face new reflected direction
                transform.rotation = Quaternion.LookRotation(reflectDir, Vector3.up);

                if (playerStats.currentHealth <= 0)
                {
                    Debug.Log("Snake died!");
                    isMoving = false;
                }
            }
            else
            {
                // Normal forward movement
                transform.position = nextPos;
            }
        }

        // steering
        if (isMoving)
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

        // store position history
        if (isMoving)
            PositionHistory.Insert(0, transform.position);


        // limit position history
        int needed = (BodyParts.Count + 1) * Gap;
        if (PositionHistory.Count > needed)
        {
            PositionHistory.RemoveAt(PositionHistory.Count - 1);
        }

        // move body parts
        if (isMoving)
        {
            int index = 1;
            foreach (var body in BodyParts)
            {
                Vector3 point = PositionHistory[Mathf.Clamp(index * Gap, 0, PositionHistory.Count - 1)];
                Vector3 moveDirection = point - body.transform.position;
                // BodySpeed (Making it the same as player speed)
                body.transform.position += moveDirection * playerStats.moveSpeed * Time.deltaTime;
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
}
