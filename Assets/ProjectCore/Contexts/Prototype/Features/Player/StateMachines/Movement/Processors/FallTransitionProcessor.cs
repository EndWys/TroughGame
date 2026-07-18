using GameCore.Movement;

namespace Prototype.Prototype
{
    public class FallTransitionProcessor : BasePlayerMovementStateProcessor
    {
        public FallTransitionProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        public override bool Execute(PlayerInputData input, out MovementStates resultState)
        {
            if (!State.GroundDetectorDataAccessor.IsGrounded)
            {
                return Complete(MovementStates.Airborne, out resultState);
            }

            return Continue(out resultState);
        }
    }
}
