using Shared;
using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyDashState : BaseEntityState<EnemyEntityState, EnemyEntityPayload>
    {
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

        public override void Enter()
        {
        }

        public override void Exit()
        {
            _dashStateMutator.StopDash();
        }

        protected override IReadOnlyList<IStateProcessor<EnemyEntityState, EnemyEntityPayload>>
            CreateProcessors()
        {
            if (_dashMovementConfig == null)
            {
                throw new InvalidOperationException(
                    "Enemy dash state requires a dash movement config reference.");
            }

            MovementConfigValidation.ValidateDash(_dashMovementConfig);

            return new IStateProcessor<EnemyEntityState, EnemyEntityPayload>[]
            {
                CreateProcessor<DashProcessor<EnemyEntityState, EnemyEntityPayload>>(
                    _dashMovementConfig,
                    (Func<Vector2>)GetDashDirection,
                    (Func<float>)GetSimulationDeltaTime,
                    EnemyEntityState.Locomotion),
            };
        }

        protected override EnemyEntityState FallbackState => EnemyEntityState.Dash;

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
