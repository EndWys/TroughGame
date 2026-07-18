using System.Collections.Generic;
using GameCore.Movement;

namespace Prototype.Prototype
{
    public class ClimbPlayerMovementState : BasePlayerMovementState
    {
        public override void Enter()
        {
            SetClimbingPhysics(true);
        }

        protected override IReadOnlyList<IMovementStateProcessor<PlayerInputData>> CreateMovementProcessors()
        {
            return new IMovementStateProcessor<PlayerInputData>[]
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
