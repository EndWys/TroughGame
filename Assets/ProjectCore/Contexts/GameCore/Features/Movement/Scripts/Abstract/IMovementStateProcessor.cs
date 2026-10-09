using System;
using Shared;

namespace ProjectCore.GameCore
{
    public interface IMovementStateProcessor<TStateType, in TStatePayload> :
        IStateProcessor<TStateType, TStatePayload>
        where TStateType : struct, Enum
        where TStatePayload : struct
    {
    }
}
