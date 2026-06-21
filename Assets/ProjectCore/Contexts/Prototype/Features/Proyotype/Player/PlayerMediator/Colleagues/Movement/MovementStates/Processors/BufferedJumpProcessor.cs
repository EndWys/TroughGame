using Domain;

namespace Prototype.Prototype
{
    public class BufferedJumpProcessor : BasePlayerMovementStateProcessor
    {
        public BufferedJumpProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        public override bool Execute(PlayerInputData input, out MovementStates resultState)
        {
            if (JumpDataAccessor.JumpBufferTimer.IsDisabled(NetworkBehaviourAccessor.ParentNetworkBehaviour.Runner))
            {
                Context.ExecuteJump();
                return Complete(MovementStates.Airborne, out resultState);
            }

            return Continue(out resultState);
        }
    }
}
