using System;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class DashProcessor<TStateType, TStatePayload> :
        IMovementStateProcessor<TStateType, TStatePayload>
        where TStateType : struct, Enum
        where TStatePayload : struct
    {
        private readonly IMovementBodyVelocityMutator _movementBodyVelocityMutator;
        private readonly IMovementCollisionBodyAccessor _movementCollisionBodyAccessor;
        private readonly IMovementObstacleProbe _movementObstacleProbe;
        private readonly IMovementStateTimerAccessor _movementStateTimerAccessor;
        private readonly DashMovementConfig _dashMovementConfig;
        private readonly Func<Vector2> _dashDirectionProvider;
        private readonly Func<float> _simulationDeltaTimeProvider;
        private readonly TStateType _nextMovementState;

        public DashProcessor(
            IMovementBodyVelocityMutator movementBodyVelocityMutator,
            IMovementCollisionBodyAccessor movementCollisionBodyAccessor,
            IMovementObstacleProbe movementObstacleProbe,
            IMovementStateTimerAccessor movementStateTimerAccessor,
            DashMovementConfig dashMovementConfig,
            Func<Vector2> dashDirectionProvider,
            Func<float> simulationDeltaTimeProvider,
            TStateType nextMovementState)
        {
            _movementBodyVelocityMutator = movementBodyVelocityMutator ??
                throw new ArgumentNullException(nameof(movementBodyVelocityMutator));
            _movementCollisionBodyAccessor = movementCollisionBodyAccessor ??
                throw new ArgumentNullException(nameof(movementCollisionBodyAccessor));
            _movementObstacleProbe = movementObstacleProbe ??
                throw new ArgumentNullException(nameof(movementObstacleProbe));
            _movementStateTimerAccessor = movementStateTimerAccessor ??
                throw new ArgumentNullException(nameof(movementStateTimerAccessor));
            _dashMovementConfig = dashMovementConfig ??
                throw new ArgumentNullException(nameof(dashMovementConfig));
            _dashDirectionProvider = dashDirectionProvider ??
                throw new ArgumentNullException(nameof(dashDirectionProvider));
            _simulationDeltaTimeProvider = simulationDeltaTimeProvider ??
                throw new ArgumentNullException(nameof(simulationDeltaTimeProvider));
            _nextMovementState = nextMovementState;
        }

        public bool Execute(TStatePayload payload, out TStateType resultState)
        {
            if (_movementStateTimerAccessor.IsStateTimerFinished || IsObstacleAhead())
            {
                _movementBodyVelocityMutator.Velocity = Vector2.zero;
                resultState = _nextMovementState;
                return true;
            }

            _movementBodyVelocityMutator.Velocity =
                _dashDirectionProvider().normalized * _dashMovementConfig.Speed;
            resultState = default;
            return false;
        }

        private bool IsObstacleAhead()
        {
            Vector2 direction = _dashDirectionProvider();
            float distance = _dashMovementConfig.Speed * _simulationDeltaTimeProvider();

            return _movementObstacleProbe.TryProbe(
                _movementCollisionBodyAccessor,
                direction,
                distance,
                out _);
        }
    }
}
