using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SimpleTopDownDrawer : MonoBehaviour
{
    public float cameraOffset = 0.6f;
    public float nearClipBuffer = 0.05f;
    public float lineWidth = 0.04f;
    public float minPointDistance = 0.02f;
    public int   maxPoints = 4096;

    public bool  closeLoopOnRelease = true;   
    public bool  loopWhileHeld = true;        
    public float closeThreshold = 0.25f;
    public float groundY = 0f;

    public int  damagePerHit = 10;
    public bool requireClosedLoop = true;

    public float  autoClearDelay = 1f;
    Coroutine _clearCo;

    LineRenderer _lr;
    Camera _cam;
    float _planeY;
    bool _isDrawing;
    bool _polygonActive;

    readonly List<Vector3> _visual = new List<Vector3>(512);
    readonly List<Vector3> _ground = new List<Vector3>(512);

    [SerializeField] SnakeController snake;

    void Awake()
    {
        _lr  = GetComponent<LineRenderer>();
        _cam = Camera.main;

        _lr.useWorldSpace = true;
        _lr.alignment = LineAlignment.View;
        _lr.textureMode = LineTextureMode.Stretch;
        _lr.shadowCastingMode = ShadowCastingMode.Off;
        _lr.receiveShadows = false;
        _lr.startWidth = lineWidth;
        _lr.endWidth   = lineWidth;
        _lr.widthCurve = AnimationCurve.Constant(0f, 1f, lineWidth);

        if (!snake) snake = FindObjectOfType<SnakeController>();
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
        if (_clearCo != null) { StopCoroutine(_clearCo); _clearCo = null; }

        _isDrawing = true;
        _polygonActive = false;

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

        if (_polygonActive)
            ApplyDamageOnceToCaptured();

        if (autoClearDelay > 0f)
            _clearCo = StartCoroutine(AutoClearAfter(autoClearDelay));
        else
            ClearStroke();
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
    }

    void Add(Vector2 screen, bool force = false)
    {
        if (_visual.Count >= maxPoints) return;

        Vector3 vWorld = ScreenToPlaneY(screen, _planeY);
        if (force || _visual.Count == 0 ||
            Vector3.Distance(_visual[_visual.Count - 1], vWorld) >= minPointDistance)
        {
            _visual.Add(vWorld);
            _lr.positionCount = _visual.Count;
            _lr.SetPosition(_visual.Count - 1, vWorld);

            _ground.Add(ScreenToPlaneY(screen, groundY));

            if (loopWhileHeld)
            {
                TryCompleteLoopWhileHeld(vWorld);
            }
        }
    }

    void TryCompleteLoopWhileHeld(Vector3 latestWorldPoint)
    {
        if (_visual.Count < 3) return;

        float d = Vector3.Distance(_visual[0], latestWorldPoint);
        if (d > closeThreshold) return;

        _lr.loop = true;
        _polygonActive = _ground.Count >= 3 && (!requireClosedLoop || _lr.loop);

        if (_polygonActive)
        {
            ApplyDamageOnceToCaptured();

            if (!_isDrawing) return;

            _visual.Clear();
            _ground.Clear();
            _lr.loop = false;
            _lr.positionCount = 0;

            _visual.Add(latestWorldPoint);
            _ground.Add(new Vector3(latestWorldPoint.x, groundY, latestWorldPoint.z));
            _lr.positionCount = 1;
            _lr.SetPosition(0, latestWorldPoint);
            _polygonActive = false;
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

    void ApplyDamageOnceToCaptured()
    {
        var enemies = FindObjectsOfType<Enemy>();
        foreach (var e in enemies)
        {
            if (!e || e.IsDead) continue;

            if (PointInsidePolygonXZ(e.transform.position, _ground))
            {
                int before = e.CurrentHealth;
                e.TakeDamage(damagePerHit);

                if (!e.IsDead && before > 0 && e.CurrentHealth <= 0)
                {
                    // hook if you want
                }

                if (e.IsDead && snake)
                {
                    snake.TriggerLunge();
                    PlayerModeSwitcher.NotifyEnemyKilled();
                    _isDrawing = false;   
                    ClearStroke();
                }
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
