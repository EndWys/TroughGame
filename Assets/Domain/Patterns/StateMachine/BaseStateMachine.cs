using System;
using System.Collections.Generic;

namespace Domain
{
    public abstract class BaseStateMachine<TStatesType, TState, TStatePayload> :  
        IStateMachine<TStatesType, TState, TStatePayload> where TStatesType : Enum
        where TState : IState<TStatesType, TStatePayload>
        where TStatePayload : struct
    {
        private Dictionary<TStatesType, TState> _states;
        
        public IReadOnlyDictionary<TStatesType, TState> States => _states;

        public abstract TStatesType CurrentState { get; protected set; }

        public abstract TStatesType PreviousState { get; protected set; }
        
        public void Init()
        {
            _states = CreateStatesDictionary();
        }

        public abstract Dictionary<TStatesType, TState> CreateStatesDictionary();

        public void ChangeState(TStatesType newState)
        {
            BeforePreviousStateExit();
            
            _states[CurrentState].Exit();

            AfterPreviousStateExit();
            
            PreviousState = CurrentState;
            CurrentState = newState;

            BeforeNextStateEnter();
            
            _states[CurrentState].Enter();

            AfterNextStateEnter();
        }

        public void UpdateStates(TStatePayload payload)
        {
            TStatesType nextState = _states[CurrentState].Tick(payload);
                
            if (EqualityComparer<TStatesType>.Default.Equals(nextState, CurrentState))
            {
                return;
            }
                
            ChangeState(nextState);
        }

        protected virtual void BeforePreviousStateExit() { }
        
        protected virtual void AfterPreviousStateExit() { }
        
        protected virtual void BeforeNextStateEnter() { }
        
        protected virtual void AfterNextStateEnter() { }
    }
}