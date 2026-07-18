using GameCore.Movement;

namespace Prototype.Prototype
{
    public class CrouchTransitionProcessor : BasePlayerMovementStateProcessor
    {
        public CrouchTransitionProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        public override bool Execute(PlayerInputData input, out MovementStates resultState)
        {
            if (input.IsCrouchPressed && State.PlayerPoseComponent.CanChangePose(PoseTypes.Crouch))
            {
                return Complete(MovementStates.Crouch, out resultState);
            }

            return Continue(out resultState);
        }
    }
}
