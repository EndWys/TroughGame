using ProjectCore.GameCore;

namespace ProjectCore.Prototype
{
    public class WallClingTransitionProcessor : BasePlayerMovementStateProcessor
    {
        public WallClingTransitionProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        public override bool Execute(PlayerInputData input, out MovementStates resultState)
        {
            if (input.MoveDirection.y > 0f && State.ClimbDetectorDataMutator.IsNearValidWall)
            {
                return Complete(MovementStates.Climb, out resultState);
            }

            return Continue(out resultState);
        }
    }
}
