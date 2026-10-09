using System;
using Shared;

namespace ProjectCore.GameCore
{
    public interface IMovementStateDataMutator<TStateType> :
        IMovementStateDataAccessor<TStateType>,
        IStateDataMutator<TStateType>
        where TStateType : struct, Enum
    {
        public void ChangeMovementState(TStateType newState);
    }
}
