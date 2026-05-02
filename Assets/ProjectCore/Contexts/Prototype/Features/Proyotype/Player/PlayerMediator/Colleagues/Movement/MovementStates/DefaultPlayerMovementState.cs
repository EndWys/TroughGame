namespace Prototype.Prototype
{
    public class DefaultPlayerMovementState : BasePlayerMovementState
    {
        public override void Enter()
        {
            
        }

        public override MovementStates Tick(PlayerInputData input)
        {
            return MovementStates.Default;
        }

        public override void Exit()
        {
            
        }
    }
}