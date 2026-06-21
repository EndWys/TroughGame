namespace Prototype.Prototype
{
    public class ClimbGroundExitProcessor : BasePlayerMovementStateProcessor
    {
        public ClimbGroundExitProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        public override bool Execute(PlayerInputData input, out MovementStates resultState)
        {
            if (GroundDetectorDataAccessor.IsGrounded && input.MoveDirection.y <= 0f)
            {
                return Complete(MovementStates.Idle, out resultState);
            }

            return Continue(out resultState);
        }
    }
}
