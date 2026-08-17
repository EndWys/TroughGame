using System.Collections.Generic;
using ProjectCore.GameCore;

namespace ProjectCore.Prototype
{
    public sealed class IdlePlayerMovementState : BasePlayerMovementState
    {
        public override void Enter() { }

        protected override IReadOnlyList<IMovementStateProcessor<PlayerInputData>> CreateMovementProcessors()
        {
            return new IMovementStateProcessor<PlayerInputData>[]
            {
                new BufferedJumpProcessor(this),
                new FallTransitionProcessor(this),
                new CrouchTransitionProcessor(this),
                new GroundedLocomotionProcessor(this),
            };
        }

        protected override MovementStates FallbackState => MovementStates.Idle;

        public override void Exit() { }
    }
}
