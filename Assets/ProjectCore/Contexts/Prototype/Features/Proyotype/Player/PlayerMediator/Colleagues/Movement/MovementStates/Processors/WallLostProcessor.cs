namespace Prototype.Prototype
{
    public class WallLostProcessor : BasePlayerMovementStateProcessor
    {
        public WallLostProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        public override bool Execute(PlayerInputData input, out MovementStates resultState)
        {
            if (!ClimbDetectorDataChanger.IsNearValidWall)
            {
                return Complete(MovementStates.Airborne, out resultState);
            }

            return Continue(out resultState);
        }
    }
}
