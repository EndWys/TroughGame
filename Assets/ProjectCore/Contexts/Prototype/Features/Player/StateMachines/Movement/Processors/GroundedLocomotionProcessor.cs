using GameCore.Movement;

namespace Prototype.Prototype
{
    public class GroundedLocomotionProcessor : BaseGroundedMovementProcessor
    {
        public GroundedLocomotionProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        protected override MovementStates GetEmptyInputState()
        {
            return MovementStates.Idle;
        }

        protected override MovementStates GetMovingState(PlayerInputData input)
        {
            return input.IsRunning ? MovementStates.Run : MovementStates.Walk;
        }

        protected override float GetTargetSpeed(PlayerInputData input)
        {
            return input.IsRunning ? State.Context.LocomotionConfig.RunSpeed : State.Context.LocomotionConfig.WalkSpeed;
        }
    }
}
