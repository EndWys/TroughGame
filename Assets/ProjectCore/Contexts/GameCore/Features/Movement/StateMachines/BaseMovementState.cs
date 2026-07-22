using Domain;
using System.Collections.Generic;

namespace ProjectCore.GameCore
{
    public abstract class BaseMovementState<TStatePayload> :
        BaseProcessorsBasedMonoBehaviourState<MovementStates, TStatePayload>
        where TStatePayload : struct
    {
        protected sealed override IReadOnlyList<IStateProcessor<MovementStates, TStatePayload>> CreateProcessors()
        {
            return CreateMovementProcessors();
        }

        protected abstract IReadOnlyList<IMovementStateProcessor<TStatePayload>> CreateMovementProcessors();
    }
}
