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

    public int   damagePerHit = 10;
    public bool  requireClosedLoop = true;

    public float autoClearDelay = 1f;
    Coroutine _clearCo;

    LineRenderer _lr;
    Camera _cam;
    float _planeY;
    bool _isDrawing;
    bool _polygonActive;

    readonly List<Vector3> _visual = new List<Vector3>(512);
    readonly List<Vector3> _ground = new List<Vector3>(512);

    [SerializeField] SnakeController snake;
    [SerializeField] private Transform pfDamagePopup;

    [SerializeField] int   minPointsToClose = 4;
    [SerializeField] float closeThresholdPixels = 42f;
    [SerializeField] float selfIntersectEpsPx = 10f;
    [SerializeField] float minPerimeterWorld = 0.25f;
    [SerializeField] float angleSumThresholdDeg = 300f;

    [SerializeField] float capturePad = 0.45f;
    [SerializeField] float sampleBoundsScale = 0.9f;

    List<Vector2> _screenPts = new List<Vector2>(512);
    Vector2 _firstScreen;

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
        if (pfDamagePopup != null) DamagePopup.SetPrefab(pfDamagePopup);
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
        _screenPts.Clear();

        _lr.loop = false;
        _lr.positionCount = 0;

        _firstScreen = screen;
        _screenPts.Add(screen);
        Add(screen, true);
    }

    void End()
    {
        _isDrawing = false;

        bool closed = false;
        if (closeLoopOnRelease && _visual.Count >= minPointsToClose)
        {
            Vector2 lastScr = WorldToScreen(_visual[_visual.Count - 1]);
            if ((lastScr - _firstScreen).sqrMagnitude <= closeThresholdPixels * closeThresholdPixels)
            {
                _lr.loop = true;
                closed = true;
            }
            else
            {
                float d = Vector3.Distance(_visual[0], _visual[_visual.Count - 1]);
                if (d <= closeThreshold) { _lr.loop = true; closed = true; }
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
        _screenPts.Clear();
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
            _screenPts.Add(screen);

            if (loopWhileHeld)
            {
                TryCompleteLoopWhileHeld(vWorld, screen);
            }
        }
    }

    void TryCompleteLoopWhileHeld(Vector3 latestWorldPoint, Vector2 latestScreen)
    {
        if (_visual.Count < minPointsToClose) return;

        bool close =
            (latestScreen - _firstScreen).sqrMagnitude <= closeThresholdPixels * closeThresholdPixels
            || SelfIntersectsScreen(_screenPts, selfIntersectEpsPx)
            || SignedAngleSumDegXZ(_ground) >= angleSumThresholdDeg
            || (PerimeterXZ(_ground) >= minPerimeterWorld &&
                (latestScreen - _firstScreen).sqrMagnitude <= (closeThresholdPixels * 1.6f) * (closeThresholdPixels * 1.6f));

        if (!close) return;

        _lr.loop = true;
        _polygonActive = _ground.Count >= 3 && (!requireClosedLoop || _lr.loop);

        if (_polygonActive)
        {
            ApplyDamageOnceToCaptured();

            if (!_isDrawing) return;

            _visual.Clear();
            _ground.Clear();
            _screenPts.Clear();
            _lr.loop = false;
            _lr.positionCount = 0;

            _visual.Add(latestWorldPoint);
            _ground.Add(new Vector3(latestWorldPoint.x, groundY, latestWorldPoint.z));
            _screenPts.Add(latestScreen);

            _lr.positionCount = 1;
            _lr.SetPosition(0, latestWorldPoint);
            _firstScreen = latestScreen;
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

    Vector2 WorldToScreen(Vector3 w)
    {
        if (!_cam) _cam = Camera.main;
        return _cam ? (Vector2)_cam.WorldToScreenPoint(w) : Vector2.zero;
    }

    Vector3 GetPopupPos(Enemy e)
    {
        var col = e.GetComponentInChildren<Collider>();
        if (col) return new Vector3(col.bounds.center.x, col.bounds.max.y, col.bounds.center.z);

        var rend = e.GetComponentInChildren<Renderer>();
        if (rend) return new Vector3(rend.bounds.center.x, rend.bounds.max.y, rend.bounds.center.z);

        return e.transform.position;
    }

    void ApplyDamageOnceToCaptured()
    {
        if (_ground.Count < 3) return;

        var enemies = FindObjectsOfType<Enemy>();
        foreach (var e in enemies)
        {
            if (!e || e.IsDead) continue;

            if (EnemyCapturedLenient(e, _ground, capturePad, sampleBoundsScale))
            {
                int before = e.CurrentHealth;
                e.TakeDamage(damagePerHit);
                int dealt = Mathf.Min(before, damagePerHit);

                if (pfDamagePopup != null && dealt > 0)
                {
                    var pos = GetPopupPos(e);
                    DamagePopup.Create(pos, dealt, false);
                }

                if (e.IsDead)
                {
                    var status = (snake ? snake.GetComponent<SnakeStatus>() : null) ?? FindObjectOfType<SnakeStatus>();
                    if (status != null) status.OnEnemyKilled();
                    PlayerModeSwitcher.NotifyEnemyKilled();
                }
            }
        }
    }

    static float PerimeterXZ(List<Vector3> pts)
    {
        float sum = 0f;
        for (int i = 1; i < pts.Count; i++)
            sum += Vector3.Distance(new Vector3(pts[i-1].x,0,pts[i-1].z), new Vector3(pts[i].x,0,pts[i].z));
        return sum;
    }

    static float SignedAngleSumDegXZ(List<Vector3> pts)
    {
        if (pts.Count < 3) return 0f;
        float total = 0f;
        for (int i = 1; i < pts.Count - 1; i++)
        {
            Vector2 a = new Vector2(pts[i].x - pts[i-1].x, pts[i].z - pts[i-1].z);
            Vector2 b = new Vector2(pts[i+1].x - pts[i].x, pts[i+1].z - pts[i].z);
            if (a.sqrMagnitude < 1e-6f || b.sqrMagnitude < 1e-6f) continue;
            a.Normalize(); b.Normalize();
            float cross = a.x * b.y - a.y * b.x;
            float dot   = Vector2.Dot(a, b);
            total += Mathf.Atan2(cross, Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;
        }
        return Mathf.Abs(total);
    }

    static bool SelfIntersectsScreen(List<Vector2> scr, float epsPx)
    {
        int n = scr.Count;
        if (n < 4) return false;
        for (int i = 0; i < n - 3; i++)
        {
            Vector2 a1 = scr[i], a2 = scr[i + 1];
            for (int j = i + 2; j < n - 1; j++)
            {
                if (j == i + 1) continue;
                Vector2 b1 = scr[j], b2 = scr[j + 1];
                if (SegmentsCloseOrIntersect(a1, a2, b1, b2, epsPx))
                    return true;
            }
        }
        return false;
    }

    static bool SegmentsCloseOrIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float eps)
    {
        if (Mathf.Max(Mathf.Min(a.x,b.x), Mathf.Min(c.x,d.x)) - Mathf.Min(Mathf.Max(a.x,b.x), Mathf.Max(c.x,d.x)) > eps) return false;
        if (Mathf.Max(Mathf.Min(a.y,b.y), Mathf.Min(c.y,d.y)) - Mathf.Min(Mathf.Max(a.y,b.y), Mathf.Max(c.y,d.y)) > eps) return false;

        float o1 = Orient(a,b,c);
        float o2 = Orient(a,b,d);
        float o3 = Orient(c,d,a);
        float o4 = Orient(c,d,b);
        if (o1 * o2 < 0f && o3 * o4 < 0f) return true;

        return DistSegToSeg(a,b,c,d) <= eps;
    }

    static float Orient(Vector2 a, Vector2 b, Vector2 c) => (b.x-a.x)*(c.y-a.y) - (b.y-a.y)*(c.x-a.x);

    static float DistSegToSeg(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        float d1 = DistPtToSeg(a,c,d);
        float d2 = DistPtToSeg(b,c,d);
        float d3 = DistPtToSeg(c,a,b);
        float d4 = DistPtToSeg(d,a,b);
        return Mathf.Min(Mathf.Min(d1,d2), Mathf.Min(d3,d4));
    }

    static float DistPtToSeg(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float len2 = Vector2.Dot(ab, ab);
        if (len2 < 1e-6f) return Vector2.Distance(p, a);
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / len2);
        Vector2 proj = a + t * ab;
        return Vector2.Distance(p, proj);
    }

    static bool EnemyCapturedLenient(Enemy e, List<Vector3> poly, float pad, float sampleScale)
    {
        Collider col = e.GetComponentInChildren<Collider>();
        Bounds b;
        if (col) b = col.bounds;
        else
        {
            var rend = e.GetComponentInChildren<Renderer>();
            b = rend ? rend.bounds : new Bounds(e.transform.position, Vector3.one * 0.5f);
        }

        Vector3 c = b.center;
        if (InsideOrNearPolygonXZ(c, poly, pad)) return true;

        float hx = b.extents.x * sampleScale;
        float hz = b.extents.z * sampleScale;

        Vector3 p0 = new Vector3(c.x - hx, c.y, c.z - hz);
        Vector3 p1 = new Vector3(c.x + hx, c.y, c.z - hz);
        Vector3 p2 = new Vector3(c.x + hx, c.y, c.z + hz);
        Vector3 p3 = new Vector3(c.x - hx, c.y, c.z + hz);

        return InsideOrNearPolygonXZ(p0, poly, pad) ||
               InsideOrNearPolygonXZ(p1, poly, pad) ||
               InsideOrNearPolygonXZ(p2, poly, pad) ||
               InsideOrNearPolygonXZ(p3, poly, pad);
    }

    static bool InsideOrNearPolygonXZ(Vector3 p, List<Vector3> poly, float pad)
    {
        if (PointInsidePolygonXZ(p, poly)) return true;
        float d = DistancePointToPolyEdgesXZ(p, poly);
        return d <= pad;
    }

    static float DistancePointToPolyEdgesXZ(Vector3 p, List<Vector3> poly)
    {
        float best = float.PositiveInfinity;
        for (int i = 0; i < poly.Count; i++)
        {
            Vector3 a = poly[i];
            Vector3 b = poly[(i + 1) % poly.Count];
            float d = DistancePointToSegmentXZ(p, a, b);
            if (d < best) best = d;
        }
        return best;
    }

    static float DistancePointToSegmentXZ(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector2 P = new Vector2(p.x, p.z);
        Vector2 A = new Vector2(a.x, a.z);
        Vector2 B = new Vector2(b.x, b.z);

        Vector2 AB = B - A;
        float len2 = Vector2.Dot(AB, AB);
        if (len2 < 1e-6f) return Vector2.Distance(P, A);

        float t = Mathf.Clamp01(Vector2.Dot(P - A, AB) / len2);
        Vector2 proj = A + t * AB;
        return Vector2.Distance(P, proj);
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
