using UnityEngine;

namespace Prototype.Prototype
{
    public class AirborneLandingProcessor : BasePlayerMovementStateProcessor
    {
        public AirborneLandingProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        public override bool Execute(PlayerInputData input, out MovementStates resultState)
        {
            if (GroundDetectorDataAccessor.IsGrounded && Context.Rigidbody.linearVelocity.y <= 0f)
            {
                Vector3 baseDirectionCheck = new Vector3(input.MoveDirection.y, 0f, input.MoveDirection.x);

                return Complete(
                    baseDirectionCheck.sqrMagnitude > 0f
                        ? input.IsRunning ? MovementStates.Run : MovementStates.Walk
                        : MovementStates.Idle,
                    out resultState);
            }

            return Continue(out resultState);
        }
    }
}
