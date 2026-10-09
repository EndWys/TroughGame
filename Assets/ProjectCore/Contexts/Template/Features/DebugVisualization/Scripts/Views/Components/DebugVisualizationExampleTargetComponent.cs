using UnityEngine;

namespace ProjectCore.Template
{
    public sealed class DebugVisualizationExampleTargetComponent : MonoBehaviour, IDebugDrawable
    {
        [SerializeField,
         DebugValue("Examples/Stats", Label = "Health", Display = DebugVisualizationValueDisplay.Hud)]
        private float _health = 100f;

        [SerializeField,
         DebugValue("Examples/Stats", Label = "Regen", Display = DebugVisualizationValueDisplay.Hud)]
        private float _regenPerSecond = 4f;

        [SerializeField,
         DebugValue("Examples/State", Label = "State", Display = DebugVisualizationValueDisplay.Hud)]
        private string _state = "Patrol";

        [SerializeField,
         DebugRadius("Examples/Vision", Label = "Vision", Filled = true,
             ValueDisplay = DebugVisualizationValueDisplay.World)]
        private float _visionRadius = 5f;

        [SerializeField,
         DebugRadius("Examples/Combat", Label = "Attack Range",
             ValueDisplay = DebugVisualizationValueDisplay.World)]
        private float _attackRange = 2f;

        [SerializeField] private float _moveRadius = 1.5f;
        [SerializeField] private float _damageInterval = 1.25f;
        [SerializeField] private float _drawInterval = 0.2f;

        private Vector3 _origin;
        private float _nextDamageTime;
        private float _nextDrawTime;

        public void Configure(
            float health,
            float regenPerSecond,
            float visionRadius,
            float attackRange,
            float moveRadius)
        {
            _health = health;
            _regenPerSecond = regenPerSecond;
            _visionRadius = visionRadius;
            _attackRange = attackRange;
            _moveRadius = moveRadius;
        }

        private void Awake()
        {
            _origin = transform.position;
        }

        private void Update()
        {
            var t = Time.time;
            var target = _origin + new Vector3(
                Mathf.Sin(t * 0.7f) * _moveRadius,
                0f,
                Mathf.Cos(t * 0.45f) * _moveRadius);

            transform.rotation = Quaternion.LookRotation(target - transform.position);
            transform.position = target;

            _health = 65f + Mathf.PingPong(t * _regenPerSecond, 35f);
            _state = Mathf.Sin(t) > 0f ? "Patrol" : "Alert";

            if (t >= _nextDamageTime)
            {
                _nextDamageTime = t + _damageInterval;
                var hitPoint = transform.position + Vector3.up * 1.35f + Random.insideUnitSphere * 0.35f;
                DebugVisualizationUtility.DrawNumber(
                    hitPoint, Random.Range(8f, 32f), "Examples/Damage", duration: 1.1f);
            }

            if (t >= _nextDrawTime)
            {
                _nextDrawTime = t + _drawInterval;
                DebugVisualizationUtility.DrawLine(
                    transform.position + Vector3.up * 0.05f,
                    transform.position + transform.forward * _attackRange + Vector3.up * 0.05f,
                    "Examples/Forward",
                    duration: _drawInterval + 0.05f);
            }
        }

        public void DrawDebug(DebugVisualizationContextAdapter context)
        {
            var center = transform.position;
            context.ZoneDisc(center, _attackRange * 0.55f, "Examples/Regen Zone");
        }
    }
}
