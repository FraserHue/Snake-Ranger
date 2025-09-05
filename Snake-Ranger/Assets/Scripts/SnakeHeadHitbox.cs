using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public class SnakeHeadHitbox : MonoBehaviour
{
    [SerializeField] private SnakeStatus snakeStatus;
    [SerializeField] private SnakeController controller;
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private int contactDamage = 5;
    [SerializeField] private float hitCooldownPerEnemy = 0.25f;
    [SerializeField] private float extraRadius = 0.0f;
    [SerializeField] private float headRadiusFallback = 0.25f;
    [SerializeField] private LayerMask enemyMask = 0;

    private readonly Collider[] _hits = new Collider[32];
    private readonly Dictionary<int, float> _lastHitTimeByEnemy = new Dictionary<int, float>(64);
    FieldInfo _headRadiusField;

    void Awake()
    {
        if (controller == null) controller = GetComponentInParent<SnakeController>();
        if (snakeStatus == null) snakeStatus = GetComponentInParent<SnakeStatus>();
        if (controller != null)
            _headRadiusField = controller.GetType().GetField("headRadius", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    }

    void Update()
    {
        if (snakeStatus == null || controller == null) return;
        if (snakeStatus.IsDead) return;
        if (controller.IsInvincible) return;

        float r = headRadiusFallback;
        if (_headRadiusField != null && _headRadiusField.FieldType == typeof(float))
            r = (float)_headRadiusField.GetValue(controller);
        float radius = Mathf.Max(0.01f, r + extraRadius);

        int count = (enemyMask.value == 0)
            ? Physics.OverlapSphereNonAlloc(transform.position, radius, _hits, ~0, QueryTriggerInteraction.Collide)
            : Physics.OverlapSphereNonAlloc(transform.position, radius, _hits, enemyMask, QueryTriggerInteraction.Collide);

        float now = Time.time;

        for (int i = 0; i < count; i++)
        {
            var col = _hits[i];
            if (col == null) continue;

            var enemy = col.GetComponentInParent<Enemy>();
            if (enemy == null || enemy.IsDead) continue;

            int id = enemy.GetInstanceID();

            float last;
            if (_lastHitTimeByEnemy.TryGetValue(id, out last))
            {
                if (now - last < hitCooldownPerEnemy) continue;
            }

            snakeStatus.TakeDamage(contactDamage);
            _lastHitTimeByEnemy[id] = now;
        }
    }
}
