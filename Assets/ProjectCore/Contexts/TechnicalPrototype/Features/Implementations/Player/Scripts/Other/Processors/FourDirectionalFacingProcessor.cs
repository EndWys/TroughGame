using System;
using Domain;
using ProjectCore.GameCore;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class FourDirectionalFacingProcessor<TStateType, TStatePayload> :
        IMovementStateProcessor<TStateType, TStatePayload>
        where TStateType : struct, Enum
        where TStatePayload : struct, ILocomotionPayload
    {
        private const float AxisThreshold = 0.01f;

        private readonly IPlayerFacingDirectionMutator _facingDirectionMutator;

        public FourDirectionalFacingProcessor(IPlayerFacingDirectionMutator facingDirectionMutator)
        {
            _facingDirectionMutator = facingDirectionMutator ??
                throw new ArgumentNullException(nameof(facingDirectionMutator));
        }

        public bool Execute(TStatePayload payload, out TStateType resultState)
        {
            if (payload.Direction.sqrMagnitude > AxisThreshold * AxisThreshold)
            {
                PlayerFacingDirection facingDirection = PlayerFacingDirectionUtility.FromVector(
                    payload.Direction,
                    _facingDirectionMutator.FacingDirection,
                    AxisThreshold);
                _facingDirectionMutator.SetFacingDirection(facingDirection);
            }

            resultState = default;
            return false;
        }
    }
}
