using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class BossPointer : MonoBehaviour
{
    [Header("Refs")]
    public Camera Cam;
    public Transform Target;
    public Canvas Canvas;
    public Graphic ArrowGraphic;

    [Header("Behavior")]
    public bool HideWhenOnScreen = true;
    public float EdgePadding = 40f;
    public float MinDistanceToHide = 1.5f;

    [Header("Smoothing")]
    public bool SmoothRotate = true;
    public float RotateLerp = 20f;

    RectTransform _rt;
    RectTransform _canvasRT;
    bool _hasCanvasCamera;
    Enemy _enemy;

    static readonly Vector3[] _corners = new Vector3[4];

    void Awake()
    {
        _rt = (RectTransform)transform;
        if (!Canvas) Canvas = GetComponentInParent<Canvas>();
        if (!ArrowGraphic) ArrowGraphic = GetComponent<Graphic>();
        if (!Cam) Cam = Camera.main;

        if (Canvas)
        {
            _canvasRT = Canvas.transform as RectTransform;
            _hasCanvasCamera = (Canvas.renderMode == RenderMode.ScreenSpaceCamera);
        }

        // start hidden until SetTarget() is called with a live boss
        SetVisible(false);
    }

    public void SetTarget(Transform t)
    {
        Target = t;
        _enemy = Target ? Target.GetComponent<Enemy>() : null;
        SetVisible(Target != null && Target.gameObject.activeInHierarchy);
    }

    public void ClearTarget()
    {
        Target = null;
        _enemy = null;
        SetVisible(false);
    }

    void LateUpdate()
    {
        if (!_rt || !Canvas || !Cam) { SetVisible(false); return; }

        // must have a valid, active target
        if (!Target || !Target.gameObject || !Target.gameObject.activeInHierarchy)
        {
            SetVisible(false);
            return;
        }

        // auto-hide if boss reports dead
        if (_enemy && _enemy.IsDead)
        {
            SetVisible(false);
            return;
        }

        // hide if extremely close to camera (prevents jitter on top of us)
        Vector3 camToTarget = Target.position - Cam.transform.position;
        if (camToTarget.sqrMagnitude < MinDistanceToHide * MinDistanceToHide)
        {
            SetVisible(false);
            return;
        }

        // screen-space position of the boss
        Vector3 screen = Cam.WorldToScreenPoint(Target.position);
        bool behind = screen.z < 0f;

        // clamp position to canvas edges with padding
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRT, screen, _hasCanvasCamera ? Canvas.worldCamera : null, out canvasPos);

        _canvasRT.GetLocalCorners(_corners);
        Vector2 min = new Vector2(_corners[0].x + EdgePadding, _corners[0].y + EdgePadding);
        Vector2 max = new Vector2(_corners[2].x - EdgePadding, _corners[2].y - EdgePadding);

        canvasPos.x = Mathf.Clamp(canvasPos.x, min.x, max.x);
        canvasPos.y = Mathf.Clamp(canvasPos.y, min.y, max.y);
        _rt.anchoredPosition = canvasPos;

        // decide visibility based on on-screen test
        bool onScreen =
            screen.z > 0f &&
            screen.x >= 0f && screen.x <= Screen.width &&
            screen.y >= 0f && screen.y <= Screen.height;

        if (HideWhenOnScreen && onScreen)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);

        // rotate arrow to point from screen center toward the boss
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 dir = ((Vector2)screen - screenCenter);

        // if behind camera, flip direction (this fixes “wrong direction sometimes”)
        if (behind) dir = -dir;

        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.up;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f; // sprite points up
        Quaternion q = Quaternion.Euler(0f, 0f, angle);

        if (SmoothRotate)
            _rt.rotation = Quaternion.Lerp(_rt.rotation, q, 1f - Mathf.Exp(-RotateLerp * Time.unscaledDeltaTime));
        else
            _rt.rotation = q;
    }

    void SetVisible(bool v)
    {
        if (ArrowGraphic) ArrowGraphic.enabled = v;
        for (int i = 0; i < transform.childCount; i++)
        {
            var g = transform.GetChild(i).GetComponent<Graphic>();
            if (g) g.enabled = v;
        }
    }
}
