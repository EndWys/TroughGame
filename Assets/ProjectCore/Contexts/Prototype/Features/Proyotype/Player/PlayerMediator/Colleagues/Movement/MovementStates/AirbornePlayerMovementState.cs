using System.Collections.Generic;
using UnityEngine;

namespace Prototype.Prototype
{
    public class AirbornePlayerMovementState : BasePlayerMovementState
    {
        public override void Enter() { }

        protected override IReadOnlyList<IPlayerMovementStateProcessor> CreateProcessors()
        {
            return new IPlayerMovementStateProcessor[]
            {
                new WallClingTransitionProcessor(this),
                new AirborneLandingProcessor(this),
                new CoyoteJumpProcessor(this),
                new AirborneMovementProcessor(this),
            };
        }

        protected override MovementStates FallbackState => MovementStates.Airborne;

        public override void Exit() { }

        public override Vector3 ApplyVelocityChangeModifier(Vector3 velocityChange)
        {
            velocityChange.y = 0;

            return velocityChange;
        }
    }
}
