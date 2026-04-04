using ProjectCore.Features.Prototype.Player;
using ProjectCore.Features.Prototype.Player.PlayerMediator;

namespace ProjectCore.Features.Proyotype.Player
{
    public abstract class BaseMovementState
    {
        protected readonly PlayerMovement Context;

        protected BaseMovementState(PlayerMovement context)
        {
            Context = context;
        }
        
        public abstract void Enter();

        public abstract EMovementState Tick(ref PlayerInputData input);

        public abstract void Exit();
    }
}