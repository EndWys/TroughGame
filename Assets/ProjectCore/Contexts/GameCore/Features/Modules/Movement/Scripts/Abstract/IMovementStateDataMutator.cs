using Domain;

namespace ProjectCore.GameCore
{
    public interface IMovementStateDataMutator :
        IMovementStateDataAccessor,
        IStateDataMutator<MovementStates>
    {
        public void ChangeMovementState(MovementStates newState);
    }
}
