using System;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class DodgeProcessor<TStateType, TStatePayload> :
        IMovementStateProcessor<TStateType, TStatePayload>
        where TStateType : struct, Enum
        where TStatePayload : struct, ILocomotionPayload
    {
        private readonly IMovementBodyVelocityMutator _movementBody;
        private readonly IMovementStateTimerAccessor _movementStateTimerAccessor;
        private readonly DodgeMovementConfig _dodgeConfig;
        private readonly TStateType _locomotionState;

        public DodgeProcessor(
            IMovementBodyVelocityMutator movementBody,
            IMovementStateTimerAccessor movementStateTimerAccessor,
            DodgeMovementConfig dodgeConfig,
            TStateType locomotionState)
        {
            _movementBody = movementBody ??
                throw new ArgumentNullException(nameof(movementBody));
            _movementStateTimerAccessor = movementStateTimerAccessor ??
                throw new ArgumentNullException(nameof(movementStateTimerAccessor));
            _dodgeConfig = dodgeConfig ??
                throw new ArgumentNullException(nameof(dodgeConfig));
            _locomotionState = locomotionState;
        }

        public bool Execute(
            TStatePayload payload,
            out TStateType resultState)
        {
            if (_movementStateTimerAccessor.IsStateTimerFinished)
            {
                _movementBody.Velocity = Vector2.zero;
                resultState = _locomotionState;
                return true;
            }

            _movementBody.Velocity =
                Vector2.ClampMagnitude(payload.Direction, 1f) *
                _dodgeConfig.Speed;
            resultState = default;
            return false;
        }
    }
}
