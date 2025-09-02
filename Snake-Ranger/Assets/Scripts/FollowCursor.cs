using UnityEngine;

[DefaultExecutionOrder(100)]
public class FollowCursor : MonoBehaviour
{
    public PlayerModeSwitcher modeSwitcher; 

    Camera _cam;
    TrailRenderer _trail;
    SimpleTopDownDrawer _drawer;
    Vector3 _vel;
    bool _weHidCursor;

    void Awake()
    {
        _cam = Camera.main;
        _trail = GetComponent<TrailRenderer>();
        _drawer = FindObjectOfType<SimpleTopDownDrawer>(true);
        if (!modeSwitcher) modeSwitcher = FindObjectOfType<PlayerModeSwitcher>(true);
    }

    void OnEnable()
    {
        SetTrail(false);
    }

    void OnDisable()
    {
        if (_weHidCursor) { Cursor.visible = true; _weHidCursor = false; }
        SetTrail(false);
    }

    void Update()
    {
        if (!_cam) _cam = Camera.main;
        if (!_cam) return;

        // always track cursor position
        Vector2 screen = (Input.touchCount > 0) ? (Vector2)Input.GetTouch(0).position : (Vector2)Input.mousePosition;
        Vector3 target = ScreenToPlaneY(screen, GetTargetY());

        transform.position = Vector3.SmoothDamp(
            transform.position, target, ref _vel, 0.01f, Mathf.Infinity, Time.unscaledDeltaTime);

        // trail only during Combat + Mouse1 held
        bool inCombat = modeSwitcher ? (modeSwitcher.currentMode == PlayerModeSwitcher.Mode.Combat) : true;
        bool mouseHeld = Input.GetMouseButton(0);
        bool shouldEmit = inCombat && mouseHeld;

        SetTrail(shouldEmit);
    }

    float GetTargetY()
    {
        // always use drawer.groundY if available
        if (_drawer != null) return _drawer.groundY;
        return 0f;
    }

    Vector3 ScreenToPlaneY(Vector2 screen, float y)
    {
        Ray r = _cam.ScreenPointToRay(screen);
        Plane p = new Plane(Vector3.up, new Vector3(0f, y, 0f));
        return p.Raycast(r, out float enter) ? r.GetPoint(enter) : transform.position;
    }

    void SetTrail(bool on)
    {
        if (_trail) _trail.emitting = on;
        if (on && !_weHidCursor) { Cursor.visible = false; _weHidCursor = true; }
        if (!on && _weHidCursor) { Cursor.visible = true; _weHidCursor = false; }
    }
}
