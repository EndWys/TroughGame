using System;

namespace Domain
{
    public interface IStateProcessor<TStatesType, in TStatePayload>
        where TStatesType : Enum
        where TStatePayload : struct
    {
        bool Execute(TStatePayload payload, out TStatesType resultState);
    }
}
