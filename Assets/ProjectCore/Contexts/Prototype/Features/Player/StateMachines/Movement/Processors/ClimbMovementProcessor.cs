using UnityEngine;
using GameCore.Movement;

namespace Prototype.Prototype
{
    public class ClimbMovementProcessor : BasePlayerMovementStateProcessor
    {
        private const float ClimbSpeed = 3f;

        public ClimbMovementProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        public override bool Execute(PlayerInputData input, out MovementStates resultState)
        {
            Vector3 wallNormal = State.ClimbDetectorDataChanger.CurrentWallNormal;
            Vector3 wallRight = Vector3.Cross(wallNormal, Vector3.up).normalized;
            Vector3 wallUp = Vector3.Cross(wallRight, wallNormal).normalized;
            Vector3 climbDirection = (wallRight * input.MoveDirection.x + wallUp * input.MoveDirection.y).normalized;

            if (climbDirection.sqrMagnitude > 0f)
            {
                ApplyVelocityChange(climbDirection * ClimbSpeed);
            }
            else
            {
                ApplyVelocityChange(Vector3.zero);
            }

            return Complete(MovementStates.Climb, out resultState);
        }
    }
}
