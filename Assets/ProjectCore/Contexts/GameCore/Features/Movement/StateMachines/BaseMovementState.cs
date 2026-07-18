using Domain;
using System.Collections.Generic;

namespace GameCore.Movement
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
