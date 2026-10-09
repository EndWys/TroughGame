using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyDashState : BaseMovementState<EnemyMovementState, EnemyMovementPayload>
    {
        [SerializeField] private TransformMovementBodyComponent _movementBody;
        [SerializeField] private DashMovementConfig _dashMovementConfig;

        private IEnemyDashStateMutator _dashStateMutator;
        private IMovementObstacleProbe _movementObstacleProbe;
        private INetworkBehaviourAccessor _networkBehaviourAccessor;

        [Inject]
        private void Construct(
            IEnemyDashStateMutator dashStateMutator,
            IMovementObstacleProbe movementObstacleProbe,
            INetworkBehaviourAccessor networkBehaviourAccessor)
        {
            _dashStateMutator = dashStateMutator ??
                throw new ArgumentNullException(nameof(dashStateMutator));
            _movementObstacleProbe = movementObstacleProbe ??
                throw new ArgumentNullException(nameof(movementObstacleProbe));
            _networkBehaviourAccessor = networkBehaviourAccessor ??
                throw new ArgumentNullException(nameof(networkBehaviourAccessor));
        }

        public override void Enter() { }

        public override void Exit()
        {
            _dashStateMutator.StopDash();
        }

        protected override IReadOnlyList<IMovementStateProcessor<EnemyMovementState, EnemyMovementPayload>>
            CreateMovementProcessors()
        {
            if (_movementBody == null)
            {
                throw new InvalidOperationException("Enemy dash state requires a movement body reference.");
            }

            if (_dashMovementConfig == null)
            {
                throw new InvalidOperationException(
                    "Enemy dash state requires a dash movement config reference.");
            }

            return new IMovementStateProcessor<EnemyMovementState, EnemyMovementPayload>[]
            {
                new DashProcessor<EnemyMovementState, EnemyMovementPayload>(
                    movementBodyVelocityMutator: _movementBody,
                    movementCollisionBodyAccessor: _movementBody,
                    movementObstacleProbe: _movementObstacleProbe,
                    movementStateTimerAccessor: _dashStateMutator,
                    dashMovementConfig: _dashMovementConfig,
                    dashDirectionProvider: GetDashDirection,
                    simulationDeltaTimeProvider: GetSimulationDeltaTime,
                    nextMovementState: EnemyMovementState.Locomotion),
            };
        }

        protected override EnemyMovementState FallbackState => EnemyMovementState.Dash;

        private Vector2 GetDashDirection()
        {
            return _dashStateMutator.DashDirection;
        }

        private float GetSimulationDeltaTime()
        {
            return _networkBehaviourAccessor.ParentNetworkBehaviour.Runner.DeltaTime;
        }
    }
}
