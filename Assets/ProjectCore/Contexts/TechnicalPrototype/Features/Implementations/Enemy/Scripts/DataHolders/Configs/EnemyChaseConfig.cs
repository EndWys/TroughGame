using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    [CreateAssetMenu(
        fileName = "Config_TechnicalPrototype_Enemy_Chase",
        menuName = "SO/Technical Prototype/Enemy/Chase")]
    public sealed class EnemyChaseConfig : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _stoppingDistance = 0.5f;
        [SerializeField, Min(0f)] private float _approachArrivalDistance = 0.1f;
        [SerializeField, Min(0f)] private float _separationDistance = 1f;
        [SerializeField, Min(0f)] private float _separationWeight = 1f;
        [SerializeField, Min(0f)] private float _obstacleProbeDistance = 1f;
        [SerializeField, Min(0f)] private float _retargetDistanceAdvantage = 2f;
        [SerializeField, Min(0f)] private float _retargetDelay = 1f;

        public float StoppingDistance => _stoppingDistance;
        public float ApproachArrivalDistance => _approachArrivalDistance;
        public float SeparationDistance => _separationDistance;
        public float SeparationWeight => _separationWeight;
        public float ObstacleProbeDistance => _obstacleProbeDistance;
        public float RetargetDistanceAdvantage => _retargetDistanceAdvantage;
        public float RetargetDelay => _retargetDelay;
    }
}
