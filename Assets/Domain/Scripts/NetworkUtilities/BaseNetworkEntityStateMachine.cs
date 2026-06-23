using System;
using System.Collections.Generic;
using Zenject;

namespace Domain
{
    public abstract class BaseNetworkEntityStateMachine<TStatesType, TState, TStatePayload> :
        BaseNetworkEntityComponent, 
        IStateMachine<TStatesType, TState, TStatePayload> where TStatesType : Enum
        where TState : IState<TStatesType, TStatePayload>
        where TStatePayload : struct
    {
        private Dictionary<TStatesType, TState> _states;
        private IStateDataChanger<TStatesType> _stateDataChanger;

        [Inject]
        private void Construct(IStateDataChanger<TStatesType> stateDataChanger)
        {
            _stateDataChanger = stateDataChanger;
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
                
            _states[_stateDataChanger.CurrentState].Exit();
    
            AfterPreviousStateExit();
            
            _stateDataChanger.ChangeState(newState);
    
            BeforeNextStateEnter();
                
            _states[_stateDataChanger.CurrentState].Enter();
    
            AfterNextStateEnter();
        }
    
        public void UpdateStates(TStatePayload payload)
        {
            TStatesType nextState = _states[_stateDataChanger.CurrentState].Tick(payload);
                    
            if (EqualityComparer<TStatesType>.Default.Equals(nextState, _stateDataChanger.CurrentState))
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
