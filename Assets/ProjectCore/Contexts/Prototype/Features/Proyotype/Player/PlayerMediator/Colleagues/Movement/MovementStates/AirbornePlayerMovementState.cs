using Domain;
using UnityEngine;

namespace Prototype.Prototype
{
    public class AirbornePlayerMovementState : BasePlayerMovementState
    {
        public override void Enter() { }

        public override MovementStates Tick(PlayerInputData input)
        {
            if (input.MoveDirection.y > 0f && ClimbDetectorDataChanger.IsNearValidWall)
            {
                return MovementStates.Climb;
            }
            
            if (GroundDetectorDataAccessor.IsGrounded && Context.Rigidbody.linearVelocity.y <= 0f)
            {
                Vector3 baseDirectionCheck = new Vector3(input.MoveDirection.y, 0f, input.MoveDirection.x);
                return baseDirectionCheck.sqrMagnitude > 0f 
                    ? (input.IsRunning ? MovementStates.Run : MovementStates.Walk) 
                    : MovementStates.Idle;
            }
            
            bool canCoyoteJump = !JumpDataAccessor.IsJumping 
                                 && JumpDataAccessor.CoyoteTimer.IsDisabled(NetworkBehaviourAccessor.ParentNetworkBehaviour.Runner) 
                                 && JumpDataAccessor.JumpBufferTimer.IsDisabled(NetworkBehaviourAccessor.ParentNetworkBehaviour.Runner);

            if (canCoyoteJump)
            {
                Context.ExecuteJump();
            }
            
            Context.Rigidbody.AddForce(Physics.gravity * Context.AirborneConfig.GravityMultiplier, ForceMode.Acceleration);
            
            Vector3 baseDirection = (Context.Rigidbody.transform.forward * input.MoveDirection.y + Context.transform.right * input.MoveDirection.x).normalized;

            if (baseDirection.sqrMagnitude > 0f)
            {
                float targetSpeed = Context.LocomotionConfig.WalkSpeed * Context.AirborneConfig.GravityMultiplier;
                ApplyVelocityChange(baseDirection * targetSpeed);
            }
            else
            {
                ApplyVelocityChange(Vector3.zero);
            }

            return MovementStates.Airborne;
        }

        public override void Exit() { }

        protected override Vector3 ApplyVelocityChangeModifier(Vector3 velocityChange)
        {
            velocityChange.y = 0;
            
            return velocityChange;
        }
    }
}