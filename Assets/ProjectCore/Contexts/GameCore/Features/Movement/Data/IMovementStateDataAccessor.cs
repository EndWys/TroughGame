using Domain;

namespace Prototype.Prototype
{
    public interface IMovementStateDataAccessor : IStateDataAccessor<MovementStates>
    {
        public MovementStates CurrentMovementStates { get; }
        public MovementStates PreviousMovementStates { get; }
    }
}
