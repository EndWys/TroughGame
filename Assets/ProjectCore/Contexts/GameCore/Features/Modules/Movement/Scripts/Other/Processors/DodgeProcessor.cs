using System;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class DodgeProcessor<TStateType, TStatePayload> :
        IMovementStateProcessor<TStateType, TStatePayload>
        where TStateType : struct, Enum
        where TStatePayload : struct, ILocomotionPayload
    {
        private readonly IMovementBodyVelocityMutator _movementBodyVelocityMutator;
        private readonly IMovementStateTimerAccessor _movementStateTimerAccessor;
        private readonly DodgeMovementConfig _dodgeMovementConfig;
        private readonly TStateType _locomotionMovementState;

        public DodgeProcessor(
            IMovementBodyVelocityMutator movementBodyVelocityMutator,
            IMovementStateTimerAccessor movementStateTimerAccessor,
            DodgeMovementConfig dodgeMovementConfig,
            TStateType locomotionMovementState)
        {
            _movementBodyVelocityMutator = movementBodyVelocityMutator ??
                throw new ArgumentNullException(nameof(movementBodyVelocityMutator));
            _movementStateTimerAccessor = movementStateTimerAccessor ??
                throw new ArgumentNullException(nameof(movementStateTimerAccessor));
            _dodgeMovementConfig = dodgeMovementConfig ??
                throw new ArgumentNullException(nameof(dodgeMovementConfig));
            _locomotionMovementState = locomotionMovementState;
        }

        public bool Execute(
            TStatePayload payload,
            out TStateType resultState)
        {
            if (_movementStateTimerAccessor.IsStateTimerFinished)
            {
                _movementBodyVelocityMutator.Velocity = Vector2.zero;
                resultState = _locomotionMovementState;
                return true;
            }

            _movementBodyVelocityMutator.Velocity =
                Vector2.ClampMagnitude(payload.Direction, 1f) *
                _dodgeMovementConfig.Speed;
            resultState = default;
            return false;
        }
    }
}
