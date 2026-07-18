using Domain;

namespace GameCore.Movement
{
    public interface IMovementStateDataAccessor : IStateDataAccessor<MovementStates>
    {
        public MovementStates CurrentMovementStates { get; }
        public MovementStates PreviousMovementStates { get; }
    }
}
