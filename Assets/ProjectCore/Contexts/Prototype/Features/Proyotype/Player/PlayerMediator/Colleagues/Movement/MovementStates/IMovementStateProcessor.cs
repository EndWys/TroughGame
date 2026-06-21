using Domain;

namespace Prototype.Prototype
{
    public interface IMovementStateProcessor<in TStatePayload> :
        IStateProcessor<MovementStates, TStatePayload>
        where TStatePayload : struct
    {
    }
}
