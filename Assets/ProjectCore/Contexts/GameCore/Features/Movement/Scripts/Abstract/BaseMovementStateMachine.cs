using System;
using System.Collections.Generic;

namespace ProjectCore.GameCore
{
    // Compatibility surface for the historical Prototype movement graph.
    public abstract class BaseMovementStateMachine<TStateType, TMovementState, TStatePayload> :
        BaseEntityStateMachine<TStateType, TMovementState, TStatePayload>
        where TStateType : struct, Enum
        where TMovementState : BaseMovementState<TStateType, TStatePayload>
        where TStatePayload : struct
    {
        public sealed override Dictionary<TStateType, TMovementState> CreateStatesDictionary()
        {
            return CreateMovementStatesDictionary();
        }

        protected sealed override bool TryGetPayload(out TStatePayload payload)
        {
            return TryGetMovementPayload(out payload);
        }

        protected sealed override void InitializeState(TMovementState state) { InitMovementState(state); }
        protected sealed override void BeforeStateUpdate(TStatePayload payload) { BeforeMovementUpdate(payload); }
        protected sealed override bool ShouldSimulate() { return ShouldSimulateMovement(); }

        protected abstract Dictionary<TStateType, TMovementState> CreateMovementStatesDictionary();
        protected abstract bool TryGetMovementPayload(out TStatePayload payload);
        protected virtual void InitMovementState(TMovementState state) { state.Init(); }
        protected virtual void BeforeMovementUpdate(TStatePayload payload) { }

        protected virtual bool ShouldSimulateMovement()
        {
            return HasSimulationAuthority;
        }
    }
}
