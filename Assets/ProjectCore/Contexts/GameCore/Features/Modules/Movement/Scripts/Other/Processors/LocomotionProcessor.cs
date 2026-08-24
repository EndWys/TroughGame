using System;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class LocomotionProcessor<TStateType, TStatePayload> :
        IMovementStateProcessor<TStateType, TStatePayload>
        where TStateType : struct, Enum
        where TStatePayload : struct, ILocomotionPayload
    {
        private readonly IMovementBodyVelocityMutator _movementBody;
        private readonly LocomotionMovementConfig _locomotionConfig;

        public LocomotionProcessor(
            IMovementBodyVelocityMutator movementBody,
            LocomotionMovementConfig locomotionConfig)
        {
            _movementBody = movementBody ??
                throw new ArgumentNullException(nameof(movementBody));
            _locomotionConfig = locomotionConfig ??
                throw new ArgumentNullException(nameof(locomotionConfig));
        }

        public bool Execute(
            TStatePayload payload,
            out TStateType resultState)
        {
            Vector2 direction = Vector2.ClampMagnitude(payload.Direction, 1f);
            _movementBody.Velocity = direction * _locomotionConfig.MaxSpeed;

            resultState = default;
            return false;
        }
    }
}
