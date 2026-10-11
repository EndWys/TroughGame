using System;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyDashTransitionProcessor :
        IMovementStateProcessor<EnemyMovementState, EnemyMovementPayload>
    {
        private readonly IMovementCollisionBodyAccessor _movementCollisionBodyAccessor;
        private readonly IMovementObstacleProbe _movementObstacleProbe;
        private readonly IEnemyDashStateMutator _dashStateMutator;
        private readonly DashMovementConfig _dashMovementConfig;

        public EnemyDashTransitionProcessor(
            IMovementCollisionBodyAccessor movementCollisionBodyAccessor,
            IMovementObstacleProbe movementObstacleProbe,
            IEnemyDashStateMutator dashStateMutator,
            DashMovementConfig dashMovementConfig)
        {
            _movementCollisionBodyAccessor = movementCollisionBodyAccessor ??
                throw new ArgumentNullException(nameof(movementCollisionBodyAccessor));
            _movementObstacleProbe = movementObstacleProbe ??
                throw new ArgumentNullException(nameof(movementObstacleProbe));
            _dashStateMutator = dashStateMutator ??
                throw new ArgumentNullException(nameof(dashStateMutator));
            _dashMovementConfig = dashMovementConfig ??
                throw new ArgumentNullException(nameof(dashMovementConfig));
        }

        public bool Execute(EnemyMovementPayload payload, out EnemyMovementState resultState)
        {
            if (!payload.IsDashRequested ||
                payload.Direction.sqrMagnitude <= Mathf.Epsilon ||
                IsDashPathBlocked(payload.Direction))
            {
                resultState = default;
                return false;
            }

            _dashStateMutator.StartDash(
                targetPosition: payload.DashTargetPosition,
                direction: payload.Direction,
                durationSeconds: _dashMovementConfig.DurationSeconds,
                cooldownSeconds: _dashMovementConfig.CooldownSeconds);
            resultState = EnemyMovementState.Dash;
            return true;
        }

        private bool IsDashPathBlocked(Vector2 dashDirection)
        {
            return _movementObstacleProbe.TryProbe(
                _movementCollisionBodyAccessor,
                dashDirection,
                _dashMovementConfig.Distance,
                out _);
        }
    }
}
