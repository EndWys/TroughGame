using GameCore.Movement;

namespace Prototype.Prototype
{
    public class WallClingTransitionProcessor : BasePlayerMovementStateProcessor
    {
        public WallClingTransitionProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        public override bool Execute(PlayerInputData input, out MovementStates resultState)
        {
            if (input.MoveDirection.y > 0f && State.ClimbDetectorDataChanger.IsNearValidWall)
            {
                return Complete(MovementStates.Climb, out resultState);
            }

            return Continue(out resultState);
        }
    }
}
