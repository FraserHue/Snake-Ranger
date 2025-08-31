using UnityEngine;

public class PlayerModeSwitcher : MonoBehaviour
{
    public enum Mode { Movement, Combat }

    public MonoBehaviour snakeController; 
    public MonoBehaviour lineDrawer;      
    public LineRenderer drawerLineRenderer;
    public float combatMoveSpeed = 1f;

    public Mode currentMode = Mode.Movement;

    float _movementOriginalSpeed = -1f;
    bool _cached = false;

    float GetMoveSpeed()
    {
        var t = snakeController;
        if (t == null) return -1f;
        var type = t.GetType();
        var f = type.GetField("MoveSpeed");
        if (f != null) return (float)f.GetValue(t);
        var p = type.GetProperty("MoveSpeed");
        if (p != null) return (float)p.GetValue(t, null);
        return -1f;
    }

    void SetMoveSpeed(float v)
    {
        var t = snakeController;
        if (t == null) return;
        var type = t.GetType();
        var f = type.GetField("MoveSpeed");
        if (f != null) { f.SetValue(t, v); return; }
        var p = type.GetProperty("MoveSpeed");
        if (p != null && p.CanWrite) { p.SetValue(t, v, null); }
    }

    void Awake()
    {
        if (lineDrawer != null && drawerLineRenderer == null)
        {
            drawerLineRenderer = lineDrawer.GetComponent<LineRenderer>();
            if (drawerLineRenderer == null)
                drawerLineRenderer = lineDrawer.GetComponentInChildren<LineRenderer>(true);
        }
    }

    void Start()
    {
        CacheOriginalMoveSpeedIfNeeded();
        ApplyMode(Mode.Movement);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            ToggleMode();
    }

    void CacheOriginalMoveSpeedIfNeeded()
    {
        if (_cached) return;
        _movementOriginalSpeed = Mathf.Max(0f, GetMoveSpeed());
        _cached = true;
    }

    public void ToggleMode()
    {
        ApplyMode(currentMode == Mode.Movement ? Mode.Combat : Mode.Movement);
    }

    public void ApplyMode(Mode mode)
    {
        currentMode = mode;
        CacheOriginalMoveSpeedIfNeeded();

        if (mode == Mode.Movement)
        {
            if (_movementOriginalSpeed >= 0f) SetMoveSpeed(_movementOriginalSpeed);
            SetDrawerEnabled(false);
        }
        else
        {
            if (combatMoveSpeed < 0f) combatMoveSpeed = 1f;
            SetMoveSpeed(combatMoveSpeed);
            SetDrawerEnabled(true);
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
}
