using Domain;
using UnityEngine;

namespace Prototype.Prototype
{
    public class ClimbJumpProcessor : BasePlayerMovementStateProcessor
    {
        public ClimbJumpProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        public override bool Execute(PlayerInputData input, out MovementStates resultState)
        {
            if (JumpDataAccessor.JumpBufferTimer.IsDisabled(NetworkBehaviourAccessor.ParentNetworkBehaviour.Runner))
            {
                Vector3 jumpDirection = (ClimbDetectorDataChanger.CurrentWallNormal + Vector3.up).normalized;

                Context.Rigidbody.linearVelocity = Vector3.zero;
                Context.ExecuteJump(jumpDirection);

                return Complete(MovementStates.Airborne, out resultState);
            }

            return Continue(out resultState);
        }
    }
}
