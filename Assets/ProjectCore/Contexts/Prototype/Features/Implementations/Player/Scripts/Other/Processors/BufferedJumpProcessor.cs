using ProjectCore.GameCore;

namespace ProjectCore.Prototype
{
    public sealed class BufferedJumpProcessor : BasePlayerMovementStateProcessor
    {
        public BufferedJumpProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        public override bool Execute(PlayerInputData input, out MovementStates resultState)
        {
            if (State.JumpDataAccessor.JumpBufferTimer.IsDisabled(State.NetworkBehaviourAccessor.ParentNetworkBehaviour.Runner))
            {
                State.Context.ExecuteJump();
                return Complete(MovementStates.Airborne, out resultState);
            }

            return Continue(out resultState);
        }
    }
}
