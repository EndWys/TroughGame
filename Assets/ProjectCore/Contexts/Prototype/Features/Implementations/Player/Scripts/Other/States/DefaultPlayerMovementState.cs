using System.Collections.Generic;
using ProjectCore.GameCore;

namespace ProjectCore.Prototype
{
    public sealed class DefaultPlayerMovementState : BasePlayerMovementState
    {
        public override void Enter()
        {
        }

        protected override IReadOnlyList<IMovementStateProcessor<MovementStates, PlayerInputData>> CreateMovementProcessors()
        {
            return new IMovementStateProcessor<MovementStates, PlayerInputData>[] { };
        }

        protected override MovementStates FallbackState => MovementStates.Default;

        public override void Exit()
        {
        }
    }
}
