using Domain;

namespace GameCore.Movement
{
    public interface IMovementStateProcessor<in TStatePayload> :
        IStateProcessor<MovementStates, TStatePayload>
        where TStatePayload : struct
    {
    }
}
