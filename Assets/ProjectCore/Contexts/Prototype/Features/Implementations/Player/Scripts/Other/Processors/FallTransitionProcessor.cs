using ProjectCore.GameCore;

namespace ProjectCore.Prototype
{
    public sealed class FallTransitionProcessor : BasePlayerMovementStateProcessor
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
