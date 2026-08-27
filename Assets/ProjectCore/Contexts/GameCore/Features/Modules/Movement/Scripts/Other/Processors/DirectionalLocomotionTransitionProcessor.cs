using System;
using Domain;

namespace ProjectCore.GameCore
{
    public sealed class DirectionalLocomotionTransitionProcessor<TStateType, TStatePayload> :
        IMovementStateProcessor<TStateType, TStatePayload>
        where TStateType : struct, Enum
        where TStatePayload : struct, ILocomotionPayload
    {
        private readonly TStateType _locomotionMovementState;

        public DirectionalLocomotionTransitionProcessor(
            TStateType locomotionMovementState)
        {
            _locomotionMovementState = locomotionMovementState;
        }

        public bool Execute(
            TStatePayload payload,
            out TStateType resultState)
        {
            if (payload.Direction.sqrMagnitude <= 0f)
            {
                resultState = default;
                return false;
            }

            resultState = _locomotionMovementState;
            return true;
        }
    }
}
