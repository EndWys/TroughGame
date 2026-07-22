using Domain;

namespace ProjectCore.GameCore
{
    public interface IMovementStateProcessor<in TStatePayload> :
        IStateProcessor<MovementStates, TStatePayload>
        where TStatePayload : struct
    {
    }
}
