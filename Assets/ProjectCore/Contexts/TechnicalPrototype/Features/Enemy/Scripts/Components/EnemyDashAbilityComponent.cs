using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyDashAbilityComponent :
        MonoBehaviour,
        IEnemyMovementAbilityContributor,
        IEnemyChaseAbilityContributor
    {
        [SerializeField] private EnemyDashState _dashState;
        [SerializeField] private TransformMovementBodyComponent _movementBody;
        [SerializeField] private EnemyDashAbilityConfig _dashAbilityConfig;

        private IEnemyDashStateMutator _dashStateMutator;
        private IEnemyDashStateAccessor _dashStateAccessor;
        private IMovementObstacleProbe _movementObstacleProbe;
        private INetworkBehaviourAccessor _networkBehaviourAccessor;

        [Inject]
        private void Construct(
            IEnemyDashStateMutator dashStateMutator,
            IEnemyDashStateAccessor dashStateAccessor,
            IMovementObstacleProbe movementObstacleProbe,
            INetworkBehaviourAccessor networkBehaviourAccessor)
        {
            _dashStateMutator = dashStateMutator ??
                throw new ArgumentNullException(nameof(dashStateMutator));
            _dashStateAccessor = dashStateAccessor ??
                throw new ArgumentNullException(nameof(dashStateAccessor));
            _movementObstacleProbe = movementObstacleProbe ??
                throw new ArgumentNullException(nameof(movementObstacleProbe));
            _networkBehaviourAccessor = networkBehaviourAccessor ??
                throw new ArgumentNullException(nameof(networkBehaviourAccessor));
        }

        public void AddMovementStates(
            IDictionary<EnemyMovementState, BaseMovementState<EnemyMovementState, EnemyMovementPayload>>
                movementStates)
        {
            if (movementStates == null)
            {
                throw new ArgumentNullException(nameof(movementStates));
            }

            movementStates.Add(EnemyMovementState.Dash, RequireDashState());
        }

        public void AddTransitionProcessors(
            EnemyMovementState sourceMovementState,
            IList<IMovementStateProcessor<EnemyMovementState, EnemyMovementPayload>> processors)
        {
            if (processors == null)
            {
                throw new ArgumentNullException(nameof(processors));
            }

            if (sourceMovementState != EnemyMovementState.Idle &&
                sourceMovementState != EnemyMovementState.Locomotion)
            {
                return;
            }

            processors.Add(new EnemyDashTransitionProcessor(
                movementCollisionBodyAccessor: RequireMovementBody(),
                movementObstacleProbe: _movementObstacleProbe,
                dashStateMutator: _dashStateMutator,
                dashMovementConfig: RequireDashAbilityConfig().DashMovementConfig));
        }

        public BaseEnemyChaseAbilityProcessor CreateChaseAbilityProcessor()
        {
            return RequireDashAbilityConfig().CreateChaseAbilityProcessor(
                _networkBehaviourAccessor,
                _dashStateAccessor);
        }

        private EnemyDashState RequireDashState()
        {
            return _dashState != null
                ? _dashState
                : throw new InvalidOperationException("Enemy dash ability requires a dash state reference.");
        }

        private TransformMovementBodyComponent RequireMovementBody()
        {
            return _movementBody != null
                ? _movementBody
                : throw new InvalidOperationException("Enemy dash ability requires a movement body reference.");
        }

        private EnemyDashAbilityConfig RequireDashAbilityConfig()
        {
            return _dashAbilityConfig != null
                ? _dashAbilityConfig
                : throw new InvalidOperationException("Enemy dash ability requires a config reference.");
        }
    }
}
