using System;
using System.Collections.Generic;

namespace Domain
{
    public interface IStateMachine<TStatesType, TState, in TStatePayload> 
        where TStatesType : Enum, IComparable
        where TState : IState<TStatesType, TStatePayload>
        where TStatePayload : struct
    {
        public IReadOnlyDictionary<TStatesType, TState> States { get; }
        public Dictionary<TStatesType, TState> CreateStatesDictionary();
        public void ChangeState(TStatesType newState);
        public void UpdateStates(TStatePayload payload);
    }
}