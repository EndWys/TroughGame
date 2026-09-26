using System;
using Fusion;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyBehaviourTargetSelectionState : EnemyBehaviourState
    {
        private EnemyInputSourceComponent _inputSource;
        private IEnemyTargetMutator _targetMutator;
        private INetworkBehaviourAccessor _networkBehaviourAccessor;
        private EnemyTargetingService _targetingService;

        [Inject]
        private void Construct(
            EnemyInputSourceComponent inputSource,
            IEnemyTargetMutator targetMutator,
            INetworkBehaviourAccessor networkBehaviourAccessor,
            EnemyTargetingService targetingService)
        {
            _inputSource = inputSource ?? throw new ArgumentNullException(nameof(inputSource));
            _targetMutator = targetMutator ?? throw new ArgumentNullException(nameof(targetMutator));
            _networkBehaviourAccessor = networkBehaviourAccessor ??
                throw new ArgumentNullException(nameof(networkBehaviourAccessor));
            _targetingService = targetingService ?? throw new ArgumentNullException(nameof(targetingService));
        }

        public override EnemyBehaviourStateType Tick(EnemyBehaviourPayload payload)
        {
            _inputSource.SetInput(default);
            Vector2 position = _networkBehaviourAccessor.ParentNetworkBehaviour.transform.position;

            if (_targetingService.TryFindNearestTarget(position, out NetworkEntityIdData targetEntityId))
            {
                _targetMutator.SetTargetEntityId(targetEntityId);
            }
            else
            {
                _targetMutator.ClearTargetEntityId();
            }

            return EnemyBehaviourStateType.Waiting;
        }
    }
}
