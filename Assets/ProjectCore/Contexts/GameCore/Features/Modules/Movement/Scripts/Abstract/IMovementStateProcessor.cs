using System;
using Domain;

namespace ProjectCore.GameCore
{
    public interface IMovementStateProcessor<TStateType, in TStatePayload> :
        IStateProcessor<TStateType, TStatePayload>
        where TStateType : struct, Enum
        where TStatePayload : struct
    {
    }
}
