using UnityEngine;

namespace Prototype.Prototype
{
    public class CrouchPlayerMovementState : BasePlayerMovementState
    {
        public override void Enter()
        {
            PoseController.SetPose(PoseTypes.Crouch);
        }

        public override MovementStates Tick(PlayerInputData input)
        {
            if (!GroundDetectorDataAccessor.IsGrounded)
            {
                return MovementStates.Airborne;
            }
            
            if (!input.IsCrouchPressed && PoseController.CanChangePose(PoseTypes.Stand))
            {
                return input.MoveDirection.sqrMagnitude > 0f ? MovementStates.Walk : MovementStates.Idle;
            }

            Vector3 baseDirection = (Context.Rigidbody.transform.forward * input.MoveDirection.y + Context.transform.right * input.MoveDirection.x).normalized;

            if (baseDirection.sqrMagnitude <= 0f)
            {
                ApplyVelocityChange(Vector3.zero);
                return MovementStates.Crouch;
            }

            Vector3 projectedDirection = Vector3.ProjectOnPlane(baseDirection, 
                GroundDetectorDataAccessor.GroundNormal).normalized;
            
            ApplyVelocityChange(projectedDirection * Context.CrouchConfig.CrouchSpeed);

            return MovementStates.Crouch;
        }

        public override void Exit()
        {
            PoseController.SetPose(PoseTypes.Stand);
        }
    }
}