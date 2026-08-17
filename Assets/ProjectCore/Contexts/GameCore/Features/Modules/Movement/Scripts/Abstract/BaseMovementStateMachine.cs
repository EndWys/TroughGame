using System.Collections.Generic;

namespace ProjectCore.GameCore
{
    public abstract class BaseMovementStateMachine<TMovementState, TStatePayload> :
        BaseNetworkEntityStateMachine<MovementStates, TMovementState, TStatePayload>
        where TMovementState : BaseMovementState<TStatePayload>
        where TStatePayload : struct
    {
        protected abstract MovementStates InitialState { get; }

        public override void Init()
        {
            if (!ShouldPerformMovement())
            {
                return;
            }

            InitializeStateMachine();

            foreach (TMovementState state in States.Values)
            {
                InitMovementState(state);
            }

            ChangeState(InitialState);
        }

        public sealed override Dictionary<MovementStates, TMovementState> CreateStatesDictionary()
        {
            return CreateMovementStatesDictionary();
        }

        public override void NetworkTick()
        {
            if (!ShouldPerformMovement())
            {
                return;
            }

            if (!TryGetMovementPayload(out TStatePayload payload))
            {
                return;
            }

            BeforeMovementUpdate(payload);
            UpdateStates(payload);
        }

        protected abstract Dictionary<MovementStates, TMovementState> CreateMovementStatesDictionary();

        protected abstract bool TryGetMovementPayload(out TStatePayload payload);

        protected virtual void InitMovementState(TMovementState state)
        {
            state.Init();
        }

        protected virtual void BeforeMovementUpdate(TStatePayload payload) { }

        protected virtual bool ShouldPerformMovement()
        {
            return ParentNetworkBehaviour.HasStateAuthority || ParentNetworkBehaviour.HasInputAuthority;
        }
    }
}
