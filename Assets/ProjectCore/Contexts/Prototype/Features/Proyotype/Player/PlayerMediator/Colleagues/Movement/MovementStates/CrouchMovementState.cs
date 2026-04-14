using UnityEngine;

namespace Prototype.Prototype
{
    public class CrouchMovementState : BaseMovementState
    {
        public CrouchMovementState(PlayerMovement context) : base(context)
        {
        }

        public override void Enter()
        {
            Context.PoseController.SetPose(PoseTypes.Crouch);
        }

        public override EMovementState Tick(ref PlayerInputData input)
        {
            if (!Context.GroundChecker.IsGrounded)
            {
                return EMovementState.Airborne;
            }
            
            if (!input.IsCrouchPressed && Context.PoseController.CanChangePose(PoseTypes.Stand))
            {
                return input.MoveDirection.sqrMagnitude > 0f ? EMovementState.Walk : EMovementState.Idle;
            }

            Vector3 baseDirection = (Context.transform.forward * input.MoveDirection.y + Context.transform.right * input.MoveDirection.x).normalized;

            if (baseDirection.sqrMagnitude <= 0f)
            {
                ApplyVelocityChange(Vector3.zero);
                return EMovementState.Crouch;
            }

            Vector3 projectedDirection = Vector3.ProjectOnPlane(baseDirection, Context.GroundChecker.GroundNormal).normalized;
            
            ApplyVelocityChange(projectedDirection * Context.CrouchConfig.CrouchSpeed);

            return EMovementState.Crouch;
        }

        public override void Exit()
        {
            Context.PoseController.SetPose(PoseTypes.Stand);
        }
    }
}