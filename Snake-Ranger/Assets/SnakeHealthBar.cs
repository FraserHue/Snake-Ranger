using UnityEngine;
using UnityEngine.UI;
using System;

public class SnakeHealthBar : MonoBehaviour
{
    [SerializeField] private SnakeStatus source;
    [SerializeField] private Image fill;
    [SerializeField] private int displayMax = 50;
    [SerializeField] private bool smooth = true;
    [SerializeField] private float smoothSpeed = 10f;

    float target;
    bool inited;

    void Awake()
    {
        if (source == null) source = FindObjectOfType<SnakeStatus>();
        if (fill != null) fill.fillAmount = 1f;
    }

    void OnEnable()
    {
        if (source != null)
        {
            source.OnHealthChanged += OnHealthChanged;
            source.OnDied += OnDied;
            inited = true;
            OnHealthChanged(source.CurrentHealth, source.MaxHealth);
        }
    }

    void OnDisable()
    {
        if (source != null)
        {
            source.OnHealthChanged -= OnHealthChanged;
            source.OnDied -= OnDied;
        }
    }

    void Update()
    {
        if (!inited || fill == null) return;
        if (smooth) fill.fillAmount = Mathf.MoveTowards(fill.fillAmount, target, smoothSpeed * Time.deltaTime);
        else fill.fillAmount = target;
    }

    void OnHealthChanged(int current, int max)
    {
        float t = Mathf.Clamp01(current / Mathf.Max(1f, (float)displayMax));
        target = t;
    }

    void OnDied()
    {
        target = 0f;
    }
}
