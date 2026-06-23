using System.Collections.Generic;
using Domain;

namespace Prototype.Prototype
{
    public class CrouchPlayerMovementState : BasePlayerMovementState
    {
        public override void Enter()
        {
            PlayerPoseComponent.SetPose(PoseTypes.Crouch);
        }

        protected override IReadOnlyList<IMovementStateProcessor<PlayerInputData>> CreateMovementProcessors()
        {
            return new IMovementStateProcessor<PlayerInputData>[]
            {
                new FallTransitionProcessor(this),
                new StandUpTransitionProcessor(this),
                new CrouchMovementProcessor(this),
            };
        }

        protected override MovementStates FallbackState => MovementStates.Crouch;

        public override void Exit()
        {
            PlayerPoseComponent.SetPose(PoseTypes.Stand);
        }
    }
}
