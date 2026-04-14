using ProjectCore.Features.Prototype.Player;
using ProjectCore.Features.Prototype.Player.PlayerMediator;
using UnityEngine;

namespace ProjectCore.Features.Proyotype.Player
{
    public abstract class BaseMovementState
    {
        protected readonly PlayerMovement Context;

        protected BaseMovementState(PlayerMovement context)
        {
            Context = context;
        }
        
        public abstract void Enter();

        public abstract EMovementState Tick(ref PlayerInputData input);

        public abstract void Exit();
        
        protected virtual void ApplyVelocityChange(Vector3 targetVelocity)
        {
            Vector3 currentVelocity = Context.Rigidbody.linearVelocity;
            
            Vector3 velocityChange = targetVelocity - currentVelocity;
            
            velocityChange = ApplyVelocityChangeModifier(velocityChange);
            
            Context.Rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        }
        
        protected void SetClimbingPhysics(bool isClimbing)
        {
            Context.Rigidbody.useGravity = !isClimbing;
            if (isClimbing)
            {
                Context.Rigidbody.linearVelocity = Vector3.zero; 
            }
        }

        protected virtual Vector3 ApplyVelocityChangeModifier(Vector3 velocityChange)
        {
            return velocityChange;
        }
    }
}