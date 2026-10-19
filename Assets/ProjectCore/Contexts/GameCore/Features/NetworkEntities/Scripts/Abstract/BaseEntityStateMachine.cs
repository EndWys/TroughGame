using System;
using ProjectCore.Template;

namespace ProjectCore.GameCore
{
    public abstract class BaseEntityStateMachine<TStateType, TState, TPayload> :
        BaseNetworkEntityStateMachine<TStateType, TState, TPayload>, IEntityStateRestart
        where TStateType : struct, Enum
        where TState : BaseProcessorsBasedMonoBehaviourState<TStateType, TPayload>
        where TPayload : struct
    {
        private bool _restartRequested;
        private TStateType? _enteredState;

        protected abstract TStateType InitialState { get; }

        protected override bool ShouldReenterCurrentState => _restartRequested;

        public void RequestRestart()
        {
            _restartRequested = true;
        }

        protected override void OnInitialize()
        {
            InitializeStateMachine();

            foreach (TState state in States.Values)
                InitializeState(state);

            if (ShouldSimulate())
                ChangeState(InitialState);
        }

        protected override void OnNetworkTick()
        {
            if (!ShouldSimulate() || !TryGetPayload(out TPayload payload))
                return;

            _restartRequested = false;

            if (!_enteredState.HasValue || !_enteredState.Value.Equals(StateData.CurrentState))
            {
                if (States[StateData.CurrentState] is BaseEntityState<TStateType, TPayload> state)
                    state.Restore();

                _enteredState = StateData.CurrentState;
            }

            BeforeStateUpdate(payload);
            UpdateStates(payload);
        }

        protected virtual void InitializeState(TState state)
        {
            state.Init();
        }

        protected abstract bool TryGetPayload(out TPayload payload);

        protected virtual void BeforeStateUpdate(TPayload payload)
        {
        }

        protected virtual bool ShouldSimulate()
        {
            return HasSimulationAuthority;
        }

        protected override void AfterNextStateEnter()
        {
            _enteredState = StateData.CurrentState;
            _restartRequested = false;
        }

        protected override void OnResetLocalRuntime()
        {
            _restartRequested = false;
            _enteredState = null;
        }
    }
}
