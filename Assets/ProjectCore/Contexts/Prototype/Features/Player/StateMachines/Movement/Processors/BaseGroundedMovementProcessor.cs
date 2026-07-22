using UnityEngine;
using ProjectCore.GameCore;

namespace ProjectCore.Prototype
{
    public abstract class BaseGroundedMovementProcessor : BasePlayerMovementStateProcessor
    {
        protected BaseGroundedMovementProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        public override bool Execute(PlayerInputData input, out MovementStates resultState)
        {
            Vector3 baseDirection = (State.Context.Rigidbody.transform.forward * input.MoveDirection.y + State.Context.Rigidbody.transform.right * input.MoveDirection.x).normalized;

            if (baseDirection.sqrMagnitude <= 0f)
            {
                ApplyVelocityChange(Vector3.zero);
                return Complete(GetEmptyInputState(), out resultState);
            }

            Vector3 projectedDirection = Vector3.ProjectOnPlane(baseDirection, State.GroundDetectorDataAccessor.GroundNormal).normalized;
            ApplyVelocityChange(projectedDirection * GetTargetSpeed(input));

            return Complete(GetMovingState(input), out resultState);
        }

        protected abstract MovementStates GetEmptyInputState();

        protected abstract MovementStates GetMovingState(PlayerInputData input);

        protected abstract float GetTargetSpeed(PlayerInputData input);
    }
}
