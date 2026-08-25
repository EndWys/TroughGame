using System;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class LocomotionProcessor<TStateType, TStatePayload> :
        IMovementStateProcessor<TStateType, TStatePayload>
        where TStateType : struct, Enum
        where TStatePayload : struct, ILocomotionPayload
    {
        private readonly IMovementBodyVelocityMutator _movementBodyVelocityMutator;
        private readonly LocomotionMovementConfig _locomotionMovementConfig;

        public LocomotionProcessor(
            IMovementBodyVelocityMutator movementBodyVelocityMutator,
            LocomotionMovementConfig locomotionMovementConfig)
        {
            _movementBodyVelocityMutator = movementBodyVelocityMutator ??
                throw new ArgumentNullException(nameof(movementBodyVelocityMutator));
            _locomotionMovementConfig = locomotionMovementConfig ??
                throw new ArgumentNullException(nameof(locomotionMovementConfig));
        }

        public bool Execute(
            TStatePayload payload,
            out TStateType resultState)
        {
            Vector2 direction = Vector2.ClampMagnitude(payload.Direction, 1f);
            _movementBodyVelocityMutator.Velocity =
                direction * _locomotionMovementConfig.MaxSpeed;

            resultState = default;
            return false;
        }
    }
}
