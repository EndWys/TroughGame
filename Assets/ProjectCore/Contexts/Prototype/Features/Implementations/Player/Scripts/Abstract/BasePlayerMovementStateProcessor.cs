using Domain;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.Prototype
{
    public abstract class BasePlayerMovementStateProcessor :
        BaseMovementStateProcessor<BasePlayerMovementState, PlayerInputData>
    {
        protected BasePlayerMovementStateProcessor(BasePlayerMovementState state) : base(state) { }

        protected void ApplyVelocityChange(Vector3 targetVelocity)
        {
            Vector3 currentVelocity = State.Context.Rigidbody.linearVelocity;
            Vector3 velocityChange = State.ApplyVelocityChangeModifier(targetVelocity - currentVelocity);

            State.Context.Rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        }

    }
}
