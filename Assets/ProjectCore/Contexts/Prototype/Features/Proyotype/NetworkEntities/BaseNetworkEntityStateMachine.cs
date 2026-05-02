using System;
using System.Collections.Generic;
using Domain;
using Zenject;

namespace Prototype.Prototype
{
    public abstract class BaseNetworkEntityStateMachine<TStatesType, TState, TStatePayload> :
        BaseNetworkEntityComponent, 
        IStateMachine<TStatesType, TState, TStatePayload> where TStatesType : Enum
        where TState : IState<TStatesType, TStatePayload>
        where TStatePayload : struct
    {
        private Dictionary<TStatesType, TState> _states;
            
        public IReadOnlyDictionary<TStatesType, TState> States => _states;
        
        private IMovementStateDataChanger<TStatesType> _movementStateDataChanger;

        [Inject]
        private void Construct(IMovementStateDataChanger<TStatesType> movementStateDataChanger)
        {
            _movementStateDataChanger = movementStateDataChanger;
        }

        protected override void Init()
        {
            _states = CreateStatesDictionary();
        }
        
        public abstract Dictionary<TStatesType, TState> CreateStatesDictionary();
    
        public void ChangeState(TStatesType newState)
        {
            BeforePreviousStateExit();
                
            _states[_movementStateDataChanger.CurrentMovementStates].Exit();
    
            AfterPreviousStateExit();
            
            _movementStateDataChanger.ChangeMovementState(newState);
    
            BeforeNextStateEnter();
                
            _states[_movementStateDataChanger.CurrentMovementStates].Enter();
    
            AfterNextStateEnter();
        }
    
        public void UpdateStates(TStatePayload payload)
        {
            TStatesType nextState = _states[_movementStateDataChanger.CurrentMovementStates].Tick(payload);
                    
            if (EqualityComparer<TStatesType>.Default.Equals(nextState, _movementStateDataChanger.CurrentMovementStates))
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