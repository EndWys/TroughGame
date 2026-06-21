using Domain;
using UnityEngine;

namespace Prototype.Prototype
{
    public abstract class BasePlayerMovementStateProcessor : IPlayerMovementStateProcessor
    {
        protected BasePlayerMovementState State { get; }

        protected PlayerMovement Context => State.Context;
        protected PoseController PoseController => State.PoseController;
        protected INetworkBehaviourAccessor NetworkBehaviourAccessor => State.NetworkBehaviourAccessor;
        protected IJumpDataAccessor JumpDataAccessor => State.JumpDataAccessor;
        protected IGroundDetectorDataAccessor GroundDetectorDataAccessor => State.GroundDetectorDataAccessor;
        protected IClimbDetectorDataChanger ClimbDetectorDataChanger => State.ClimbDetectorDataChanger;

        protected BasePlayerMovementStateProcessor(BasePlayerMovementState state)
        {
            State = state;
        }

        public abstract bool Execute(PlayerInputData input, out MovementStates resultState);

        protected void ApplyVelocityChange(Vector3 targetVelocity)
        {
            Vector3 currentVelocity = Context.Rigidbody.linearVelocity;
            Vector3 velocityChange = State.ApplyVelocityChangeModifier(targetVelocity - currentVelocity);

            Context.Rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        }

        protected static bool Continue(out MovementStates resultState)
        {
            resultState = default;
            return false;
        }

        protected static bool Complete(MovementStates state, out MovementStates resultState)
        {
            resultState = state;
            return true;
        }
    }
}
