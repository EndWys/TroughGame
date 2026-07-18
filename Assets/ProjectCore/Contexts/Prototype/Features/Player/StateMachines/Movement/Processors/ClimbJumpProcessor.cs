using Domain;
using GameCore.Movement;
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
            if (State.JumpDataAccessor.JumpBufferTimer.IsDisabled(State.NetworkBehaviourAccessor.ParentNetworkBehaviour.Runner))
            {
                Vector3 jumpDirection = (State.ClimbDetectorDataChanger.CurrentWallNormal + Vector3.up).normalized;

                State.Context.Rigidbody.linearVelocity = Vector3.zero;
                State.Context.ExecuteJump(jumpDirection);

                return Complete(MovementStates.Airborne, out resultState);
            }

            return Continue(out resultState);
        }
    }
}
