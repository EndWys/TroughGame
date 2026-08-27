using System;
using Domain;

namespace ProjectCore.GameCore
{
    public interface IMovementStateDataAccessor<TStateType> : IStateDataAccessor<TStateType>
        where TStateType : struct, Enum
    {
        public TStateType CurrentMovementStates { get; }
        public TStateType PreviousMovementStates { get; }
    }
}
