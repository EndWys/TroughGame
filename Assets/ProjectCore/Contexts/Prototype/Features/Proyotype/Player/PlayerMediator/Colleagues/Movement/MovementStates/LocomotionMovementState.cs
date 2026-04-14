using UnityEngine;

namespace Prototype.Prototype
{
    public class LocomotionMovementState : BaseMovementState
    {
        public LocomotionMovementState(PlayerMovement context) : base(context)
        {
        }

        public override void Enter() { }

        public override EMovementState Tick(PlayerInputData input)
        {
            if (!Context.JumpBufferTimer.ExpiredOrNotRunning(Context.Runner))
            {
                Context.ExecuteJump();
                return EMovementState.Airborne;
            }
            
            if (!Context.GroundChecker.IsGrounded)
            {
                return EMovementState.Airborne;
            }
            
            if (input.IsCrouchPressed && Context.PoseController.CanChangePose(PoseTypes.Crouch))
            {
                return EMovementState.Crouch;
            }
            
            Vector3 baseDirection = (Context.transform.forward * input.MoveDirection.y + Context.transform.right * input.MoveDirection.x).normalized;
            
            if (baseDirection.sqrMagnitude <= 0f)
            {
                ApplyVelocityChange(Vector3.zero);
                return EMovementState.Idle;
            }
            
            Vector3 projectedDirection = Vector3.ProjectOnPlane(baseDirection, Context.GroundChecker.GroundNormal).normalized;
            
            float targetSpeed = input.IsRunning ? Context.LocomotionConfig.RunSpeed : Context.LocomotionConfig.WalkSpeed;
            ApplyVelocityChange(projectedDirection * targetSpeed);

            return input.IsRunning ? EMovementState.Run : EMovementState.Walk;
        }

        public override void Exit() { }
    }
}