namespace Prototype.Prototype
{
    public class DefaultMovementState : BaseMovementState
    {
        public DefaultMovementState(PlayerMovement context) : base(context) { }

        public override void Enter()
        {
            
        }

        public override EMovementState Tick(PlayerInputData input)
        {
            return EMovementState.Idle;
        }

        public override void Exit()
        {
            
        }
    }
}