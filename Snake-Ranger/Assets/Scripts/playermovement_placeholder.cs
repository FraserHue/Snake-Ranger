using UnityEngine;
public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;
    CharacterController cc;
    void Awake() { cc = GetComponent<CharacterController>(); }
    void Update()
    {
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");
        Vector3 dir = new Vector3(h, 0f, v);
        if (dir.sqrMagnitude > 0.001f) transform.forward = dir;
        cc.SimpleMove(dir.normalized * speed);
    }
}
