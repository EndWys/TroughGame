using System.Collections.Generic;

namespace Prototype.Prototype
{
    public class ClimbPlayerMovementState : BasePlayerMovementState
    {
        public override void Enter()
        {
            SetClimbingPhysics(true);
        }

        protected override IReadOnlyList<IPlayerMovementStateProcessor> CreateProcessors()
        {
            return new IPlayerMovementStateProcessor[]
            {
                new ClimbCancelProcessor(this),
                new ClimbGroundExitProcessor(this),
                new ClimbJumpProcessor(this),
                new WallLostProcessor(this),
                new ClimbMovementProcessor(this),
            };
        }

        protected override MovementStates FallbackState => MovementStates.Climb;

        public override void Exit()
        {
            SetClimbingPhysics(false);
        }
    }
}
