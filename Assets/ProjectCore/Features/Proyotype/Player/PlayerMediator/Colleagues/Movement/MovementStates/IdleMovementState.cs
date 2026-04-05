using ProjectCore.Features.Prototype.Player;
using ProjectCore.Features.Prototype.Player.PlayerMediator;
using UnityEngine;

namespace ProjectCore.Features.Proyotype.Player
{
    public class IdleMovementState : BaseMovementState
    {
        public IdleMovementState(PlayerMovement context) : base(context) { }

        public override void Enter() { }

        public override EMovementState Tick(ref PlayerInputData input)
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
            
            if (input.MoveDirection.sqrMagnitude > 0f)
            {
                return input.IsRunning ? EMovementState.Run : EMovementState.Walk;
            }
            
            Context.ApplyVelocityChange(Vector3.zero);

            return EMovementState.Idle;
        }

        public override void Exit() { }
    }
}