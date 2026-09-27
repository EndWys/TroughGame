using System;
using Fusion;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyBehaviourChasingState : EnemyBehaviourState
    {
        [SerializeField] private EnemyChaseConfig _chaseConfig;

        private EnemyInputSourceComponent _inputSource;
        private IEnemyTargetAccessor _targetAccessor;
        private IEnemyTargetMutator _targetMutator;
        private INetworkBehaviourAccessor _networkBehaviourAccessor;
        private EnemyTargetingService _targetingService;

        [Inject]
        private void Construct(
            EnemyInputSourceComponent inputSource,
            IEnemyTargetAccessor targetAccessor,
            IEnemyTargetMutator targetMutator,
            INetworkBehaviourAccessor networkBehaviourAccessor,
            EnemyTargetingService targetingService)
        {
            _inputSource = inputSource ?? throw new ArgumentNullException(nameof(inputSource));
            _targetAccessor = targetAccessor ?? throw new ArgumentNullException(nameof(targetAccessor));
            _targetMutator = targetMutator ?? throw new ArgumentNullException(nameof(targetMutator));
            _networkBehaviourAccessor = networkBehaviourAccessor ??
                throw new ArgumentNullException(nameof(networkBehaviourAccessor));
            _targetingService = targetingService ?? throw new ArgumentNullException(nameof(targetingService));
        }

        public override EnemyBehaviourStateType Tick(EnemyBehaviourPayload payload)
        {
            if (_chaseConfig == null)
            {
                throw new InvalidOperationException("Enemy chasing state requires a chase config reference.");
            }

            if (!_targetingService.TryGetTargetPosition(
                    _targetAccessor.TargetEntityId,
                    out Vector2 targetPosition))
            {
                _inputSource.SetInput(default);
                _targetMutator.ClearTargetEntityId();
                return EnemyBehaviourStateType.TargetSelection;
            }

            Vector2 position = _networkBehaviourAccessor.ParentNetworkBehaviour.transform.position;
            Vector2 direction = targetPosition - position;

            if (direction.sqrMagnitude <= _chaseConfig.StoppingDistance * _chaseConfig.StoppingDistance)
            {
                _inputSource.SetInput(default);
                return EnemyBehaviourStateType.Chasing;
            }

            _inputSource.SetInput(new EnemyInputData(direction.normalized));
            return EnemyBehaviourStateType.Chasing;
        }
    }
}
