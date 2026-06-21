using System.Collections.Generic;

namespace Prototype.Prototype
{
    public class IdlePlayerMovementState : BasePlayerMovementState
    {
        public override void Enter() { }

        protected override IReadOnlyList<IPlayerMovementStateProcessor> CreateProcessors()
        {
            return new IPlayerMovementStateProcessor[]
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
