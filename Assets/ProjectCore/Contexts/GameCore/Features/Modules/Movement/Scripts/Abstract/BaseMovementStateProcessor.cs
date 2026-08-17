using System;
using Domain;

namespace ProjectCore.GameCore
{
    public abstract class BaseMovementStateProcessor<TStateType, TState, TStatePayload> :
        BaseStateProcessor<TStateType, TState, TStatePayload>,
        IMovementStateProcessor<TStateType, TStatePayload>
        where TStateType : struct, Enum
        where TState : IState<TStateType, TStatePayload>
        where TStatePayload : struct
    {
        protected BaseMovementStateProcessor(TState state) : base(state)
        {
        }
    }
}
