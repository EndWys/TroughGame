namespace Prototype.Prototype
{
    public class CrouchMovementProcessor : BaseGroundedMovementProcessor
    {
        public CrouchMovementProcessor(BasePlayerMovementState state) : base(state)
        {
        }

        protected override MovementStates GetEmptyInputState()
        {
            return MovementStates.Crouch;
        }

        protected override MovementStates GetMovingState(PlayerInputData input)
        {
            return MovementStates.Crouch;
        }

        protected override float GetTargetSpeed(PlayerInputData input)
        {
            return State.Context.CrouchConfig.CrouchSpeed;
        }
    }
}
