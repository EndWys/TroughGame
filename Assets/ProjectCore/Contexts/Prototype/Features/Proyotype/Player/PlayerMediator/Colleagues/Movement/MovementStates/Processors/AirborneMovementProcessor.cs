using UnityEngine;

namespace Prototype.Prototype
{
    public class AirborneMovementProcessor : BasePlayerMovementStateProcessor
    {
        public AirborneMovementProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        public override bool Execute(PlayerInputData input, out MovementStates resultState)
        {
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

            return Complete(MovementStates.Airborne, out resultState);
        }
    }
}
