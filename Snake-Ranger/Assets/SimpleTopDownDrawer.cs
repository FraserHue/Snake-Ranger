using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class SimpleTopDownCaptureDrawerRepeat : MonoBehaviour
{
    // ---- Drawing ----
    public float cameraOffset = 0.6f;
    public float nearClipBuffer = 0.05f;
    public float lineWidth = 0.04f;
    public float minPointDistance = 0.02f;
    public int   maxPoints = 4096;
    public bool  closeLoopOnRelease = true;
    public float closeThreshold = 0.25f;
    public float groundY = 0f;

    // ---- Capture ----
    public string enemyTag = "Enemy";
    public bool   requireClosedLoop = true;
    public bool   hitContinuouslyWhileInside = true;
    public float  hitInterval = 0.5f;      // seconds between hits per enemy (0 = every frame)

    // ---- Auto clear ----
    public float  autoClearDelay = 1f;     // seconds after release to clear (set 0 to keep)
    Coroutine _clearCo;

    public IReadOnlyList<Vector3> VisualPoints => _visual;
    public IReadOnlyList<Vector3> GroundPoints => _ground;
    public bool IsDrawing => _isDrawing;

    LineRenderer _lr;
    Camera _cam;
    float _planeY;
    bool _isDrawing;
    bool _polygonActive;

    readonly List<Vector3> _visual = new List<Vector3>(512);
    readonly List<Vector3> _ground = new List<Vector3>(512);
    readonly Dictionary<Transform, float> _nextHitTime = new Dictionary<Transform, float>();
    static readonly List<Transform> _tmpToRemove = new List<Transform>(16);

    void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _cam = Camera.main;

        // Material is NOT set here; assign a URP material in the LineRenderer inspector.
        _lr.useWorldSpace = true;
        _lr.alignment = LineAlignment.View;
        _lr.textureMode = LineTextureMode.Stretch;
        _lr.shadowCastingMode = ShadowCastingMode.Off;
        _lr.receiveShadows = false;
        _lr.startWidth = lineWidth;
        _lr.endWidth   = lineWidth;
        _lr.widthCurve = AnimationCurve.Constant(0f, 1f, lineWidth);
    }

    void Update()
    {
        UpdatePlaneY();

        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            Handle(t.phase == TouchPhase.Began,
                   t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary,
                   t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled,
                   t.position);
        }
        else
        {
            Handle(Input.GetMouseButtonDown(0),
                   Input.GetMouseButton(0),
                   Input.GetMouseButtonUp(0),
                   Input.mousePosition);
        }

        if (!_isDrawing && _polygonActive && hitContinuouslyWhileInside)
            TickDamage();
    }

    void UpdatePlaneY()
    {
        if (!_cam) _cam = Camera.main;
        if (!_cam) return;

        float minOff = _cam.nearClipPlane + nearClipBuffer;
        float used   = Mathf.Max(cameraOffset, minOff);
        _planeY      = _cam.transform.position.y - used;
    }

    void Handle(bool down, bool held, bool up, Vector2 screen)
    {
        if (down) Begin(screen);
        if (_isDrawing && held) Add(screen);
        if (_isDrawing && up) End();
    }

    void Begin(Vector2 screen)
    {
        // cancel any pending clear
        if (_clearCo != null) { StopCoroutine(_clearCo); _clearCo = null; }

        _isDrawing = true;
        _polygonActive = false;
        _nextHitTime.Clear();

        _visual.Clear();
        _ground.Clear();
        _lr.loop = false;
        _lr.positionCount = 0;
        Add(screen, true);
    }

    void End()
    {
        _isDrawing = false;

        bool closed = false;
        if (closeLoopOnRelease && _visual.Count >= 3)
        {
            float d = Vector3.Distance(_visual[0], _visual[_visual.Count - 1]);
            if (d <= closeThreshold)
            {
                _lr.loop = true;
                closed = true;
            }
        }

        _polygonActive = _ground.Count >= 3 && (!requireClosedLoop || _lr.loop || closed);

        if (_polygonActive) DoDamageOnce(); // immediate tick on release

        // schedule clear
        if (autoClearDelay > 0f)
            _clearCo = StartCoroutine(AutoClearAfter(autoClearDelay));
    }

    IEnumerator AutoClearAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearStroke();
        _clearCo = null;
    }

    void ClearStroke()
    {
        _visual.Clear();
        _ground.Clear();
        _lr.loop = false;
        _lr.positionCount = 0;
        _polygonActive = false;
        _nextHitTime.Clear();
    }

    void Add(Vector2 screen, bool force = false)
    {
        if (_visual.Count >= maxPoints) return;

        Vector3 v = ScreenToPlaneY(screen, _planeY);
        if (force || _visual.Count == 0 ||
            Vector3.Distance(_visual[_visual.Count - 1], v) >= minPointDistance)
        {
            _visual.Add(v);
            _lr.positionCount = _visual.Count;
            _lr.SetPosition(_visual.Count - 1, v);

            _ground.Add(ScreenToPlaneY(screen, groundY));
        }
    }

    Vector3 ScreenToPlaneY(Vector2 screen, float y)
    {
        if (!_cam) _cam = Camera.main;
        if (!_cam) return Vector3.zero;

        Ray r = _cam.ScreenPointToRay(screen);
        Plane p = new Plane(Vector3.up, new Vector3(0f, y, 0f));
        return p.Raycast(r, out float enter) ? r.GetPoint(enter) : Vector3.zero;
    }

    // ---- Damage ----
    void DoDamageOnce()
    {
        var enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        foreach (var e in enemies)
            if (PointInsidePolygonXZ(e.transform.position, _ground))
                Debug.Log($"{e.name} took damage");
    }

    void TickDamage()
    {
        float now = Time.time;
        var enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        // clean nulls
        _tmpToRemove.Clear();
        foreach (var kvp in _nextHitTime)
            if (kvp.Key == null) _tmpToRemove.Add(kvp.Key);
        for (int i = 0; i < _tmpToRemove.Count; i++)
            _nextHitTime.Remove(_tmpToRemove[i]);

        foreach (var e in enemies)
        {
            var tr = e.transform;
            if (!PointInsidePolygonXZ(tr.position, _ground)) continue;

            if (!_nextHitTime.TryGetValue(tr, out float next)) next = 0f;
            if (now >= next)
            {
                Debug.Log($"{e.name} took damage");
                _nextHitTime[tr] = now + Mathf.Max(0f, hitInterval);
            }
        }
    }

    static bool PointInsidePolygonXZ(Vector3 world, List<Vector3> poly)
    {
        int n = poly.Count;
        if (n < 3) return false;
        bool inside = false;
        float px = world.x, pz = world.z;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            float ax = poly[i].x, az = poly[i].z;
            float bx = poly[j].x, bz = poly[j].z;
            bool cross = ((az > pz) != (bz > pz)) &&
                         (px < (bx - ax) * (pz - az) / ((bz - az) + Mathf.Epsilon) + ax);
            if (cross) inside = !inside;
        }
        return inside;
    }
}
