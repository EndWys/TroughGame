using System.Collections.Generic;

namespace Prototype.Prototype
{
    public class CrouchPlayerMovementState : BasePlayerMovementState
    {
        public override void Enter()
        {
            PoseController.SetPose(PoseTypes.Crouch);
        }

        protected override IReadOnlyList<IPlayerMovementStateProcessor> CreateProcessors()
        {
            return new IPlayerMovementStateProcessor[]
            {
                new FallTransitionProcessor(this),
                new StandUpTransitionProcessor(this),
                new CrouchMovementProcessor(this),
            };
        }

        protected override MovementStates FallbackState => MovementStates.Crouch;

        public override void Exit()
        {
            PoseController.SetPose(PoseTypes.Stand);
        }
    }
}
