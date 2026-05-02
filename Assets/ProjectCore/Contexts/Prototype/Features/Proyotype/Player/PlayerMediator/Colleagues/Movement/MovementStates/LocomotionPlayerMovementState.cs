using Domain;
using UnityEngine;

namespace Prototype.Prototype
{
    public class LocomotionPlayerMovementState : BasePlayerMovementState
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
            
            Vector3 baseDirection = (Context.Rigidbody.transform.forward * input.MoveDirection.y + Context.Rigidbody.transform.right * input.MoveDirection.x).normalized;
            
            if (baseDirection.sqrMagnitude <= 0f)
            {
                ApplyVelocityChange(Vector3.zero);
                return MovementStates.Idle;
            }
            
            Vector3 projectedDirection = Vector3.ProjectOnPlane(baseDirection, 
                GroundDetectorDataAccessor.GroundNormal).normalized;
            
            float targetSpeed = input.IsRunning ? Context.LocomotionConfig.RunSpeed : Context.LocomotionConfig.WalkSpeed;
            ApplyVelocityChange(projectedDirection * targetSpeed);

            return input.IsRunning ? MovementStates.Run : MovementStates.Walk;
        }

        public override void Exit() { }
    }
}