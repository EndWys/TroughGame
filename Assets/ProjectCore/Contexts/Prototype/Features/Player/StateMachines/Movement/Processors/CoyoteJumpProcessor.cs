using Domain;
using ProjectCore.GameCore;
using ProjectCore.GameCore;

namespace ProjectCore.Prototype
{
    public class CoyoteJumpProcessor : BasePlayerMovementStateProcessor
    {
        public CoyoteJumpProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        public override bool Execute(PlayerInputData input, out MovementStates resultState)
        {
            bool canCoyoteJump = !State.JumpDataAccessor.IsJumping
                                 && State.JumpDataAccessor.CoyoteTimer.IsDisabled(State.NetworkBehaviourAccessor.ParentNetworkBehaviour.Runner)
                                 && State.JumpDataAccessor.JumpBufferTimer.IsDisabled(State.NetworkBehaviourAccessor.ParentNetworkBehaviour.Runner);

            if (canCoyoteJump)
            {
                State.Context.ExecuteJump();
            }

            return Continue(out resultState);
        }
    }
}
