using ProjectCore.GameCore;

namespace ProjectCore.Prototype
{
    public sealed class ClimbCancelProcessor : BasePlayerMovementStateProcessor
    {
        public ClimbCancelProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        public override bool Execute(PlayerInputData input, out MovementStates resultState)
        {
            if (input.IsCrouchPressed)
            {
                return Complete(MovementStates.Airborne, out resultState);
            }

            return Continue(out resultState);
        }
    }
}
