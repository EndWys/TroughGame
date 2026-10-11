using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.Prototype
{
    public sealed class AirbornePlayerMovementState : BasePlayerMovementState
    {
        public override void Enter() { }

        protected override IReadOnlyList<IMovementStateProcessor<MovementStates, PlayerInputData>> CreateMovementProcessors()
        {
            return new IMovementStateProcessor<MovementStates, PlayerInputData>[]
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
