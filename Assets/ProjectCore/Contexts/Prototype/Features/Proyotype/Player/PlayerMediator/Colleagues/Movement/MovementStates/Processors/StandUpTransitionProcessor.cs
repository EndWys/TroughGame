namespace Prototype.Prototype
{
    public class StandUpTransitionProcessor : BasePlayerMovementStateProcessor
    {
        public StandUpTransitionProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        public override bool Execute(PlayerInputData input, out MovementStates resultState)
        {
            if (!input.IsCrouchPressed && State.PoseController.CanChangePose(PoseTypes.Stand))
            {
                return Complete(input.MoveDirection.sqrMagnitude > 0f ? MovementStates.Walk : MovementStates.Idle, out resultState);
            }

            return Continue(out resultState);
        }
    }
}
