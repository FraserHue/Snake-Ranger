using UnityEngine;
using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{

    public float MoveSpeed = 5f;
    public float SteerSpeed = 300f;
    public int BodySpeed = 5;
    public int Gap = 10;

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
        GrowSnake();
        GrowSnake();
        GrowSnake();
        GrowSnake();
        GrowSnake();
        GrowSnake();
        GrowSnake();
        GrowSnake();
        GrowSnake();
        GrowSnake();
        aimPoint = transform.position;
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.S)) isMoving = false;
        if (Input.GetKeyDown(KeyCode.W)) isMoving = true;

        // forward movement
        if (isMoving)
            transform.position += transform.forward * MoveSpeed * Time.deltaTime;

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

        // move body parts
        if (isMoving)
        {
            int index = 1;
            foreach (var body in BodyParts) {
                Vector3 point = PositionHistory[Mathf.Min(index * Gap, PositionHistory.Count - 1)];
                Vector3 moveDirection = point - body.transform.position;
                body.transform.position += moveDirection * BodySpeed * Time.deltaTime;
                body.transform.LookAt(point);
                index++;
            }
        }
    }

    private void GrowSnake() {
        GameObject body = Instantiate(BodyPrefab);
        BodyParts.Add(body);
    }
}
