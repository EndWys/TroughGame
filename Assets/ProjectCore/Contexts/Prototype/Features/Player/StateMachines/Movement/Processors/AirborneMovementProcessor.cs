using UnityEngine;
using ProjectCore.GameCore;

namespace ProjectCore.Prototype
{
    public class AirborneMovementProcessor : BasePlayerMovementStateProcessor
    {
        public AirborneMovementProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        public override bool Execute(PlayerInputData input, out MovementStates resultState)
        {
            State.Context.Rigidbody.AddForce(Physics.gravity * State.Context.AirborneConfig.GravityMultiplier, ForceMode.Acceleration);

            Vector3 baseDirection = (State.Context.Rigidbody.transform.forward * input.MoveDirection.y + State.Context.transform.right * input.MoveDirection.x).normalized;

            if (baseDirection.sqrMagnitude > 0f)
            {
                float targetSpeed = State.Context.LocomotionConfig.WalkSpeed * State.Context.AirborneConfig.GravityMultiplier;
                ApplyVelocityChange(baseDirection * targetSpeed);
            }
            else
            {
                ApplyVelocityChange(Vector3.zero);
            }

            return Complete(MovementStates.Airborne, out resultState);
        }
    }
}
