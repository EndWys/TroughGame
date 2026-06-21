using Domain;

namespace Prototype.Prototype
{
    public class CoyoteJumpProcessor : BasePlayerMovementStateProcessor
    {
        public CoyoteJumpProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        public override bool Execute(PlayerInputData input, out MovementStates resultState)
        {
            bool canCoyoteJump = !JumpDataAccessor.IsJumping
                                 && JumpDataAccessor.CoyoteTimer.IsDisabled(NetworkBehaviourAccessor.ParentNetworkBehaviour.Runner)
                                 && JumpDataAccessor.JumpBufferTimer.IsDisabled(NetworkBehaviourAccessor.ParentNetworkBehaviour.Runner);

            if (canCoyoteJump)
            {
                Context.ExecuteJump();
            }

            return Continue(out resultState);
        }
    }
}
