using System;

namespace Prototype.Prototype
{
    public interface IMovementStateDataChanger<TStateType> : IMovementStateDataAccessor<TStateType> where TStateType : Enum
    {
        public void ChangeMovementState(TStateType newState);
    }
}