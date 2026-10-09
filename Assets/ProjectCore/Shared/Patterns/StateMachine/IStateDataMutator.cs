using System;

namespace Shared
{
    public interface IStateDataMutator<TStatesType> : IStateDataAccessor<TStatesType> where TStatesType : Enum
    {
        public void ChangeState(TStatesType newState);
    }
}
