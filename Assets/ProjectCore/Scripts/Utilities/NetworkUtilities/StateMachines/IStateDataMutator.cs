using System;

namespace ProjectCore.GameCore
{
    public interface IStateDataMutator<TStatesType> : IStateDataAccessor<TStatesType> where TStatesType : Enum
    {
        public void ChangeState(TStatesType newState);
    }
}
