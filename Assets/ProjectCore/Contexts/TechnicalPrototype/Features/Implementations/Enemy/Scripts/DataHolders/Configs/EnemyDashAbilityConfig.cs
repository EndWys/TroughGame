using System;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    [CreateAssetMenu(
        fileName = "Config_TechnicalPrototype_Enemy_DashAbility",
        menuName = "SO/TechnicalPrototype/Enemy/Dash Ability")]
    public sealed class EnemyDashAbilityConfig : ScriptableObject
    {
        [SerializeField] private DashMovementConfig _dashMovementConfig;
        [SerializeField, Min(0f)] private float _targetOffset = 0.15f;
        [SerializeField, Min(0f)] private float _startDistanceTolerance = 0.2f;

        public DashMovementConfig DashMovementConfig => _dashMovementConfig;

        public BaseEnemyChaseAbilityProcessor CreateChaseAbilityProcessor(
            INetworkBehaviourAccessor networkBehaviourAccessor,
            IEnemyDashStateAccessor dashStateAccessor)
        {
            if (_dashMovementConfig == null)
            {
                throw new InvalidOperationException(
                    "Enemy dash ability requires a dash movement config reference.");
            }

            if (networkBehaviourAccessor == null)
            {
                throw new InvalidOperationException(
                    "Enemy dash ability requires a network behaviour accessor.");
            }

            if (dashStateAccessor == null)
            {
                throw new ArgumentNullException(nameof(dashStateAccessor));
            }

            return new EnemyDashChaseAbilityProcessor(
                networkBehaviourAccessor: networkBehaviourAccessor,
                dashStateAccessor: dashStateAccessor,
                dashMovementConfig: _dashMovementConfig,
                targetOffset: _targetOffset,
                startDistanceTolerance: _startDistanceTolerance);
        }
    }
}
