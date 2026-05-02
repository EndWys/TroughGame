using Domain;
using UnityEngine;

namespace Prototype.Prototype
{
    public class ClimbPlayerMovementState : BasePlayerMovementState
    {
        public override void Enter()
        {
            SetClimbingPhysics(true);
        }

        public override MovementStates Tick(PlayerInputData input)
        {
            if (input.IsCrouchPressed)
            {
                return MovementStates.Airborne;
            }
            
            if (GroundDetectorDataAccessor.IsGrounded && input.MoveDirection.y <= 0f)
            {
                return MovementStates.Idle;
            }
            
            if (JumpDataAccessor.JumpBufferTimer.IsDisabled(NetworkBehaviourAccessor.ParentNetworkBehaviour.Runner))
            {
                Vector3 jumpDirection = (ClimbDetectorDataChanger.CurrentWallNormal + Vector3.up).normalized;
                
                Context.Rigidbody.linearVelocity = Vector3.zero;
                
                Context.ExecuteJump(jumpDirection);
                
                return MovementStates.Airborne;
            }
            
            if (!ClimbDetectorDataChanger.IsNearValidWall)
            {
                return MovementStates.Airborne;
            }
            
            Vector3 wallNormal = ClimbDetectorDataChanger.CurrentWallNormal;

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

            return MovementStates.Climb;
        }

        public override void Exit()
        {
            SetClimbingPhysics(false);
        }
    }
}