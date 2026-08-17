using System;

namespace Domain
{
    public abstract class BaseStateProcessor<TStatesType, TState, TStatePayload> :
        IStateProcessor<TStatesType, TStatePayload>
        where TStatesType : Enum
        where TState : IState<TStatesType, TStatePayload>
        where TStatePayload : struct
    {
        protected TState State { get; }

        protected BaseStateProcessor(TState state)
        {
            State = state;
        }

        public abstract bool Execute(TStatePayload payload, out TStatesType resultState);

        protected static bool Continue(out TStatesType resultState)
        {
            resultState = default;
            return false;
        }

        protected static bool Complete(TStatesType state, out TStatesType resultState)
        {
            resultState = state;
            return true;
        }
    }
}
