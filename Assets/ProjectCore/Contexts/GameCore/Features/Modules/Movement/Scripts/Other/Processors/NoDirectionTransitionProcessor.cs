using System;
using Domain;

namespace ProjectCore.GameCore
{
    public sealed class NoDirectionTransitionProcessor<TStateType, TStatePayload> :
        IMovementStateProcessor<TStateType, TStatePayload>
        where TStateType : struct, Enum
        where TStatePayload : struct, ILocomotionPayload
    {
        private readonly TStateType _idleMovementState;

        public NoDirectionTransitionProcessor(
            TStateType idleMovementState)
        {
            _idleMovementState = idleMovementState;
        }

        public bool Execute(
            TStatePayload payload,
            out TStateType resultState)
        {
            if (payload.Direction.sqrMagnitude > 0f)
            {
                resultState = default;
                return false;
            }

            resultState = _idleMovementState;
            return true;
        }
    }
}
