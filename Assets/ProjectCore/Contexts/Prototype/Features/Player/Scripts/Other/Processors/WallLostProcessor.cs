using ProjectCore.GameCore;

namespace ProjectCore.Prototype
{
    public sealed class WallLostProcessor : BasePlayerMovementStateProcessor
    {
        public WallLostProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        public override bool Execute(PlayerInputData input, out MovementStates resultState)
        {
            if (!State.ClimbDetectorDataMutator.IsNearValidWall)
            {
                return Complete(MovementStates.Airborne, out resultState);
            }

            return Continue(out resultState);
        }
    }
}
