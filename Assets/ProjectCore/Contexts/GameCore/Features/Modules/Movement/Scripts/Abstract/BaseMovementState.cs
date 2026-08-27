using System;
using System.Collections.Generic;
using Domain;

namespace ProjectCore.GameCore
{
    public abstract class BaseMovementState<TStateType, TStatePayload> :
        BaseProcessorsBasedMonoBehaviourState<TStateType, TStatePayload>
        where TStateType : struct, Enum
        where TStatePayload : struct
    {
        protected sealed override IReadOnlyList<IStateProcessor<TStateType, TStatePayload>> CreateProcessors()
        {
            return CreateMovementProcessors();
        }

        protected abstract IReadOnlyList<IMovementStateProcessor<TStateType, TStatePayload>>
            CreateMovementProcessors();
    }
}
