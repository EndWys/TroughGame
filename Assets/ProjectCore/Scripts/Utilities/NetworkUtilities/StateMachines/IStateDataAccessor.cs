using System;

namespace ProjectCore.GameCore
{
    public interface IStateDataAccessor<out TStatesType> where TStatesType : Enum
    {
        public TStatesType CurrentState { get; }
        public TStatesType PreviousState { get; }
    }
}
