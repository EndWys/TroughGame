using System.Collections.Generic;

namespace Prototype.Prototype
{
    public class LocomotionPlayerMovementState : BasePlayerMovementState
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

        protected override MovementStates FallbackState => MovementStates.Walk;

        public override void Exit() { }
    }
}
