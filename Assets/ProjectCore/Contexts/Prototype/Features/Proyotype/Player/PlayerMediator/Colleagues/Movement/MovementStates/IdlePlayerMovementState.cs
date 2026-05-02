using Domain;
using UnityEngine;

namespace Prototype.Prototype
{
    public class IdlePlayerMovementState : BasePlayerMovementState
    {
        public override void Enter() { }

        public override MovementStates Tick(PlayerInputData input)
        {
            if (JumpDataAccessor.JumpBufferTimer.IsDisabled(NetworkBehaviourAccessor.ParentNetworkBehaviour.Runner))
            {
                Context.ExecuteJump();
                return MovementStates.Airborne;
            }

            if (!GroundDetectorDataAccessor.IsGrounded)
            {
                return MovementStates.Airborne;
            }
            
            if (input.IsCrouchPressed && PoseController.CanChangePose(PoseTypes.Crouch))
            {
                return MovementStates.Crouch;
            }
            
            if (input.MoveDirection.sqrMagnitude > 0f)
            {
                return input.IsRunning ? MovementStates.Run : MovementStates.Walk;
            }
            
            ApplyVelocityChange(Vector3.zero);

            return MovementStates.Idle;
        }

        public override void Exit() { }
    }
}