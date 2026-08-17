using System;
using System.Collections.Generic;
using Domain;
using Zenject;

namespace ProjectCore.GameCore
{
    public abstract class BaseNetworkEntityStateMachine<TStatesType, TState, TStatePayload> :
        BaseNetworkEntityComponent, 
        IStateMachine<TStatesType, TState, TStatePayload> where TStatesType : Enum
        where TState : IState<TStatesType, TStatePayload>
        where TStatePayload : struct
    {
        private Dictionary<TStatesType, TState> _states;
        private IStateDataMutator<TStatesType> _stateDataMutator;

        [Inject]
        private void Construct(IStateDataMutator<TStatesType> stateDataMutator)
        {
            _stateDataMutator = stateDataMutator;
        }

        public IReadOnlyDictionary<TStatesType, TState> States => _states;

        protected void InitializeStateMachine()
        {
            _states = CreateStatesDictionary();
        }
        
        public abstract Dictionary<TStatesType, TState> CreateStatesDictionary();
    
        public void ChangeState(TStatesType newState)
        {
            BeforePreviousStateExit();
                
            _states[_stateDataMutator.CurrentState].Exit();
    
            AfterPreviousStateExit();
            
            _stateDataMutator.ChangeState(newState);
    
            BeforeNextStateEnter();
                
            _states[_stateDataMutator.CurrentState].Enter();
    
            AfterNextStateEnter();
        }
    
        public void UpdateStates(TStatePayload payload)
        {
            TStatesType nextState = _states[_stateDataMutator.CurrentState].Tick(payload);
                    
            if (EqualityComparer<TStatesType>.Default.Equals(nextState, _stateDataMutator.CurrentState))
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
