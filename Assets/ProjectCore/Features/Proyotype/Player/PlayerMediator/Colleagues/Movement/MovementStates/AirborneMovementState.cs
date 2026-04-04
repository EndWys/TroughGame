using ProjectCore.Features.Prototype.Player;
using ProjectCore.Features.Prototype.Player.PlayerMediator;
using UnityEngine;

namespace ProjectCore.Features.Proyotype.Player
{
    public class AirborneMovementState : BaseMovementState
    {
        public AirborneMovementState(PlayerMovement context) : base(context) { }

        public override void Enter() { }

        public override EMovementState Tick(ref PlayerInputData input)
        {
            if (Context.GroundChecker.IsGrounded && Context.Rigidbody.linearVelocity.y <= 0f)
            {
                Vector3 baseDirectionCheck = new Vector3(input.MoveDirection.y, 0f, input.MoveDirection.x);
                return baseDirectionCheck.sqrMagnitude > 0f 
                    ? (input.IsRunning ? EMovementState.Run : EMovementState.Walk) 
                    : EMovementState.Idle;
            }
            
            bool canCoyoteJump = !Context.IsJumping 
                                 && !Context.CoyoteTimer.Expired(Context.Runner) 
                                 && !Context.JumpBufferTimer.Expired(Context.Runner);

            if (canCoyoteJump)
            {
                Context.ExecuteJump();
            }
            
            Context.Rigidbody.AddForce(Physics.gravity * Context.AirborneConfig.GravityMultiplier, ForceMode.Acceleration);
            
            Vector3 baseDirection = (Context.transform.forward * input.MoveDirection.y + Context.transform.right * input.MoveDirection.x).normalized;

            if (baseDirection.sqrMagnitude > 0f)
            {
                float targetSpeed = Context.LocomotionConfig.WalkSpeed * Context.AirborneConfig.GravityMultiplier;
                Context.ApplyVelocityChange(baseDirection * targetSpeed);
            }
            else
            {
                Context.ApplyVelocityChange(Vector3.zero);
            }

            return EMovementState.Airborne;
        }

        public override void Exit() { }
    }
}