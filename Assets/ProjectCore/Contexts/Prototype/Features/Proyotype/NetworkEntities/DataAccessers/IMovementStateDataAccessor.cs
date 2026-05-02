using System;

namespace Prototype.Prototype
{
    public interface IMovementStateDataAccessor<out TStatesType> where TStatesType : Enum
    {
        public TStatesType CurrentMovementStates { get; }
        public TStatesType PreviousMovementStates { get; }
    }
}