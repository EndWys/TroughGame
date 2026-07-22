using Domain;

namespace ProjectCore.GameCore
{
    public abstract class BaseMovementStateProcessor<TState, TStatePayload> :
        BaseStateProcessor<MovementStates, TState, TStatePayload>,
        IMovementStateProcessor<TStatePayload>
        where TState : IState<MovementStates, TStatePayload>
        where TStatePayload : struct
    {
        protected BaseMovementStateProcessor(TState state) : base(state)
        {
        }
    }
}
