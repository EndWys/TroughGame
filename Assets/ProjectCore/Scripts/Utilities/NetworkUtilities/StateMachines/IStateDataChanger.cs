using System;

namespace Domain
{
    public interface IStateDataChanger<TStatesType> : IStateDataAccessor<TStatesType> where TStatesType : Enum
    {
        public void ChangeState(TStatesType newState);
    }
}
