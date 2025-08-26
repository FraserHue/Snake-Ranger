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
    }

    void Update()
    {

        // forward movement
        transform.position += transform.forward * MoveSpeed * Time.deltaTime;

        // steering
        float steerDirection = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up * steerDirection * SteerSpeed * Time.deltaTime);

        // store position history
        PositionHistory.Insert(0, transform.position);

        // move body parts
        int index = 1;
        foreach (var body in BodyParts) {
            Vector3 point = PositionHistory[Mathf.Min(index * Gap, PositionHistory.Count - 1)];
            Vector3 moveDirection = point - body.transform.position;
            body.transform.position += moveDirection * BodySpeed * Time.deltaTime;
            body.transform.LookAt(point);
            index++;
        }
    }

    private void GrowSnake() {
        GameObject body = Instantiate(BodyPrefab);
        BodyParts.Add(body);
    }
}
