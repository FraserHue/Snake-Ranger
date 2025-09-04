using UnityEngine;
using System.Collections;
using System.Reflection;

public class PlayerModeSwitcher : MonoBehaviour
{
    public enum Mode { Movement, Combat }

    public MonoBehaviour snakeController;
    public MonoBehaviour lineDrawer;
    public LineRenderer drawerLineRenderer;
    public Camera targetCamera;

    public int defaultLength = 3;
    public int minLengthForScale = 3;
    public int maxLengthForScale = 20;

    public float fovAtMinLen = 40f;
    public float fovAtMaxLen = 85f;

    public float orthoAtMinLen = 5f;
    public float orthoAtMaxLen = 18f;

    public Mode currentMode = Mode.Movement;

    float _defaultFOV = -1f;
    float _defaultOrtho = -1f;

    Coroutine _zoomCo;

    public static PlayerModeSwitcher Instance { get; private set; }

    // NEW: cache SnakeStatus to grant lunge charges (no auto-lunge)
    [SerializeField] private SnakeStatus snakeStatus;

    void Awake()
    {
        Instance = this;

        if (snakeController == null) snakeController = FindObjectOfType<SnakeController>();
        if (lineDrawer == null)
        {
            var foundDrawer = FindObjectOfType<MonoBehaviour>();
            var drawerTyped = FindObjectOfType<SimpleTopDownDrawer>();
            if (drawerTyped != null) lineDrawer = drawerTyped;
        }
        if (lineDrawer != null && drawerLineRenderer == null)
        {
            drawerLineRenderer = lineDrawer.GetComponent<LineRenderer>()
                                 ?? lineDrawer.GetComponentInChildren<LineRenderer>(true);
        }

        if (targetCamera == null) targetCamera = Camera.main;

        if (targetCamera != null)
        {
            if (targetCamera.orthographic) _defaultOrtho = targetCamera.orthographicSize;
            else _defaultFOV = targetCamera.fieldOfView;
        }

        if (snakeStatus == null) snakeStatus = FindObjectOfType<SnakeStatus>();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        ApplyMode(Mode.Movement);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            ToggleMode();

        if (Input.GetKeyDown(KeyCode.S)) SetIsMoving(false);
        if (Input.GetKeyDown(KeyCode.W)) SetIsMoving(true);
    }

    public void ToggleMode()
    {
        ApplyMode(currentMode == Mode.Movement ? Mode.Combat : Mode.Movement);
    }

    public void ApplyMode(Mode mode)
    {
        currentMode = mode;

        if (mode == Mode.Movement)
        {
            SetIsMoving(true);
            SetDrawerEnabled(false);
            SmoothRestoreCameraView();
        }
        else
        {
            SetIsMoving(false);
            SetDrawerEnabled(true);
            ApplyCombatCameraViewSmooth();
        }
    }

    void SetDrawerEnabled(bool enabled)
    {
        if (lineDrawer != null) lineDrawer.enabled = enabled;

        if (drawerLineRenderer != null)
        {
            drawerLineRenderer.enabled = enabled;
            if (!enabled) drawerLineRenderer.positionCount = 0;
        }
    }

    void ApplyCombatCameraViewSmooth()
    {
        if (targetCamera == null) return;

        int len = Mathf.Max(1, GetSnakeLength());
        float t = Mathf.InverseLerp(minLengthForScale, maxLengthForScale, len);

        if (targetCamera.orthographic)
        {
            float targetSize = Mathf.Lerp(orthoAtMinLen, orthoAtMaxLen, t);
            StartZoomOrtho(targetSize, 1f);
        }
        else
        {
            float targetFOV = Mathf.Lerp(fovAtMinLen, fovAtMaxLen, t);
            StartZoomFOV(targetFOV, 1f);
        }
    }

    void SmoothRestoreCameraView()
    {
        if (targetCamera == null) return;

        if (targetCamera.orthographic && _defaultOrtho > 0f)
            StartZoomOrtho(_defaultOrtho, 1f);
        else if (!targetCamera.orthographic && _defaultFOV > 0f)
            StartZoomFOV(_defaultFOV, 1f);
    }

    void StartZoomFOV(float targetFOV, float duration)
    {
        if (_zoomCo != null) StopCoroutine(_zoomCo);
        _zoomCo = StartCoroutine(ZoomFOVCo(targetFOV, duration));
    }

    IEnumerator ZoomFOVCo(float targetFOV, float duration)
    {
        float start = targetCamera.fieldOfView;
        float t = 0f;
        duration = Mathf.Max(0.0001f, duration);
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            targetCamera.fieldOfView = Mathf.Lerp(start, targetFOV, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        targetCamera.fieldOfView = targetFOV;
        _zoomCo = null;
    }

    void StartZoomOrtho(float targetSize, float duration)
    {
        if (_zoomCo != null) StopCoroutine(_zoomCo);
        _zoomCo = StartCoroutine(ZoomOrthoCo(targetSize, duration));
    }

    IEnumerator ZoomOrthoCo(float targetSize, float duration)
    {
        float start = targetCamera.orthographicSize;
        float t = 0f;
        duration = Mathf.Max(0.0001f, duration);
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            targetCamera.orthographicSize = Mathf.Lerp(start, targetSize, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        targetCamera.orthographicSize = targetSize;
        _zoomCo = null;
    }

    public static void NotifyEnemyKilled()
    {
        if (Instance != null) Instance.OnEnemyKilled();
    }

    public void OnEnemyKilled()
    {
        // Grant a lunge charge, do NOT lunge here.
        if (snakeStatus != null) snakeStatus.OnEnemyKilled();

        // Keep your existing behavior of returning to movement after a kill.
        ApplyMode(Mode.Movement);
    }

    void SetIsMoving(bool value)
    {
        if (snakeController == null) return;

        var t = snakeController.GetType();

        var p = t.GetProperty("IsMoving", BindingFlags.Public | BindingFlags.Instance);
        if (p != null && p.CanWrite)
        {
            p.SetValue(snakeController, value, null);
            return;
        }

        var f = t.GetField("isMoving", BindingFlags.Instance | BindingFlags.NonPublic);
        if (f != null)
        {
            f.SetValue(snakeController, value);
        }
    }

    int GetSnakeLength()
    {
        if (snakeController == null) return defaultLength;
        var type = snakeController.GetType();

        var fLen = type.GetField("Length") ?? type.GetField("length") ??
                   type.GetField("SegmentCount") ?? type.GetField("segmentCount");
        if (fLen != null)
        {
            var v = fLen.GetValue(snakeController);
            if (v is int i) return i;
            if (v is float f) return Mathf.RoundToInt(f);
        }

        var pLen = type.GetProperty("Length") ?? type.GetProperty("length") ??
                   type.GetProperty("SegmentCount") ?? type.GetProperty("segmentCount");
        if (pLen != null)
        {
            var v = pLen.GetValue(snakeController, null);
            if (v is int i) return i;
            if (v is float f) return Mathf.RoundToInt(f);
        }

        var fBodies = type.GetField("BodyParts") ?? type.GetField("Segments") ??
                      type.GetField("bodyParts") ?? type.GetField("segments");
        if (fBodies != null)
        {
            var list = fBodies.GetValue(snakeController) as System.Collections.ICollection;
            if (list != null) return list.Count;
        }

        var pBodies = type.GetProperty("BodyParts") ?? type.GetProperty("Segments");
        if (pBodies != null)
        {
            var listObj = pBodies.GetValue(snakeController, null) as System.Collections.ICollection;
            if (listObj != null) return listObj.Count;
        }

        return defaultLength;
    }
}
