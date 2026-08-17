using Domain;

namespace ProjectCore.GameCore
{
    public interface IMovementStateDataAccessor : IStateDataAccessor<MovementStates>
    {
        public MovementStates CurrentMovementStates { get; }
        public MovementStates PreviousMovementStates { get; }
    }
}
