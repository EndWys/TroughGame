using System.Collections.Generic;
using GameCore.Movement;

namespace Prototype.Prototype
{
    public class DefaultPlayerMovementState : BasePlayerMovementState
    {
        public override void Enter()
        {
        }

        protected override IReadOnlyList<IMovementStateProcessor<PlayerInputData>> CreateMovementProcessors()
        {
            return new IMovementStateProcessor<PlayerInputData>[] { };
        }

        protected override MovementStates FallbackState => MovementStates.Default;

        public override void Exit()
        {
        }
    }
}
