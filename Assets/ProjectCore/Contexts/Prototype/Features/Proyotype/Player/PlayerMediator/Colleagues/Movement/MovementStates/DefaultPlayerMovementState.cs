using System.Collections.Generic;

namespace Prototype.Prototype
{
    public class DefaultPlayerMovementState : BasePlayerMovementState
    {
        public override void Enter()
        {
        }

        protected override IReadOnlyList<IPlayerMovementStateProcessor> CreateProcessors()
        {
            return new IPlayerMovementStateProcessor[] { };
        }

        protected override MovementStates FallbackState => MovementStates.Default;

        public override void Exit()
        {
        }
    }
}
