using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    [CreateAssetMenu(
        fileName = "Config_TechnicalPrototype_Enemy_Chase",
        menuName = "SO/Technical Prototype/Enemy/Chase")]
    public sealed class EnemyChaseConfig : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _stoppingDistance = 0.5f;

        public float StoppingDistance => _stoppingDistance;
    }
}
