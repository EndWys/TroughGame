using System;
using System.Collections.Generic;

namespace ProjectCore.GameCore
{
    public abstract class BaseMovementStateMachine<TStateType, TMovementState, TStatePayload> :
        BaseNetworkEntityStateMachine<TStateType, TMovementState, TStatePayload>
        where TStateType : struct, Enum
        where TMovementState : BaseMovementState<TStateType, TStatePayload>
        where TStatePayload : struct
    {
        protected abstract TStateType InitialState { get; }

        protected override void OnInitialize()
        {
            InitializeStateMachine();

            foreach (TMovementState state in States.Values)
            {
                InitMovementState(state);
            }

            if (ShouldSimulateMovement())
            {
                ChangeState(InitialState);
            }
        }

        public sealed override Dictionary<TStateType, TMovementState> CreateStatesDictionary()
        {
            return CreateMovementStatesDictionary();
        }

        protected override void OnNetworkTick()
        {
            if (!ShouldSimulateMovement())
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

        protected abstract Dictionary<TStateType, TMovementState> CreateMovementStatesDictionary();

        protected abstract bool TryGetMovementPayload(out TStatePayload payload);

        protected virtual void InitMovementState(TMovementState state)
        {
            state.Init();
        }

        protected virtual void BeforeMovementUpdate(TStatePayload payload) { }

        protected virtual bool ShouldSimulateMovement()
        {
            return ParentNetworkBehaviour.HasStateAuthority || ParentNetworkBehaviour.HasInputAuthority;
        }
    }
}
