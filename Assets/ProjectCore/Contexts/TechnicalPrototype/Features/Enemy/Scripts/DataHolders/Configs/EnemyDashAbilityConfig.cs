using System;
using ProjectCore.GameCore;
using ProjectCore.Template;
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

        public BaseEnemyChaseAbilityProcessor CreateChaseAbilityProcessor(IClassFactory factory)
        {
            if (_dashMovementConfig == null) throw new InvalidOperationException("Dash movement config is required.");
            return factory.Create<EnemyDashChaseAbilityProcessor>(new object[] { _dashMovementConfig, _targetOffset, _startDistanceTolerance });
        }
    }
}
