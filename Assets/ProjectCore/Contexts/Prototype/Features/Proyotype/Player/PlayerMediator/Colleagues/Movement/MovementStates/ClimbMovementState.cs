using UnityEngine;

namespace Prototype.Prototype
{
    public class ClimbMovementState : BaseMovementState
    {
        public ClimbMovementState(PlayerMovement context) : base(context) { }

        public override void Enter()
        {
            SetClimbingPhysics(true);
        }

        public override EMovementState Tick(PlayerInputData input)
        {
            if (input.IsCrouchPressed)
            {
                return EMovementState.Airborne;
            }
            
            if (Context.GroundChecker.IsGrounded && input.MoveDirection.y <= 0f)
            {
                return EMovementState.Idle;
            }
            
            if (!Context.JumpBufferTimer.ExpiredOrNotRunning(Context.Runner))
            {
                Vector3 jumpDirection = (Context.ClimbingChecker.CurrentWallNormal + Vector3.up).normalized;
                
                Context.Rigidbody.linearVelocity = Vector3.zero;
                
                Context.ExecuteJump(jumpDirection);
                
                return EMovementState.Airborne;
            }
            
            if (!Context.ClimbingChecker.NearValidWall)
            {
                return EMovementState.Airborne;
            }
            
            Vector3 wallNormal = Context.ClimbingChecker.CurrentWallNormal;

            Vector3 wallRight = Vector3.Cross(wallNormal, Vector3.up).normalized;
            Vector3 wallUp = Vector3.Cross(wallRight, wallNormal).normalized;
            
            Vector3 climbDirection = (wallRight * input.MoveDirection.x + wallUp * input.MoveDirection.y).normalized;

            if (climbDirection.sqrMagnitude > 0f)
            {
                float climbSpeed = 3f; 
                ApplyVelocityChange(climbDirection * climbSpeed);
            }
            else
            {
                ApplyVelocityChange(Vector3.zero);
            }

            return EMovementState.Climb;
        }

        public override void Exit()
        {
            SetClimbingPhysics(false);
        }
    }
}