using System;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class ObstacleAvoidanceLocomotionProcessor<TStateType, TStatePayload> :
        IMovementStateProcessor<TStateType, TStatePayload>
        where TStateType : struct, Enum
        where TStatePayload : struct, ILocomotionPayload
    {
        private const float TargetDirectionWeight = 0.25f;

        private readonly IMovementBodyVelocityMutator _movementBodyVelocityMutator;
        private readonly IMovementCollisionBodyAccessor _movementCollisionBodyAccessor;
        private readonly IMovementObstacleProbe _movementObstacleProbe;
        private readonly LocomotionMovementConfig _locomotionMovementConfig;
        private readonly float _obstacleProbeDistance;

        public ObstacleAvoidanceLocomotionProcessor(
            IMovementBodyVelocityMutator movementBodyVelocityMutator,
            IMovementCollisionBodyAccessor movementCollisionBodyAccessor,
            IMovementObstacleProbe movementObstacleProbe,
            LocomotionMovementConfig locomotionMovementConfig,
            float obstacleProbeDistance)
        {
            _movementBodyVelocityMutator = movementBodyVelocityMutator ??
                throw new ArgumentNullException(nameof(movementBodyVelocityMutator));
            _movementCollisionBodyAccessor = movementCollisionBodyAccessor ??
                throw new ArgumentNullException(nameof(movementCollisionBodyAccessor));
            _movementObstacleProbe = movementObstacleProbe ??
                throw new ArgumentNullException(nameof(movementObstacleProbe));
            _locomotionMovementConfig = locomotionMovementConfig ??
                throw new ArgumentNullException(nameof(locomotionMovementConfig));
            _obstacleProbeDistance = Mathf.Max(0f, obstacleProbeDistance);
        }

        public bool Execute(TStatePayload payload, out TStateType resultState)
        {
            Vector2 direction = Vector2.ClampMagnitude(payload.Direction, 1f);

            if (direction.sqrMagnitude > Mathf.Epsilon &&
                _movementObstacleProbe.TryProbe(
                    _movementCollisionBodyAccessor,
                    direction,
                    _obstacleProbeDistance,
                    out Vector2 obstacleNormal))
            {
                direction = GetAvoidanceDirection(direction, obstacleNormal);
            }

            _movementBodyVelocityMutator.Velocity = direction * _locomotionMovementConfig.MaxSpeed;
            resultState = default;
            return false;
        }

        private static Vector2 GetAvoidanceDirection(Vector2 direction, Vector2 obstacleNormal)
        {
            Vector2 clockwiseTangent = new(obstacleNormal.y, -obstacleNormal.x);
            Vector2 counterClockwiseTangent = -clockwiseTangent;
            float clockwiseAlignment = Vector2.Dot(direction, clockwiseTangent);
            float counterClockwiseAlignment = Vector2.Dot(direction, counterClockwiseTangent);
            Vector2 tangent = clockwiseAlignment >= counterClockwiseAlignment
                ? clockwiseTangent
                : counterClockwiseTangent;

            return Vector2.ClampMagnitude(tangent + direction * TargetDirectionWeight, 1f);
        }
    }
}
