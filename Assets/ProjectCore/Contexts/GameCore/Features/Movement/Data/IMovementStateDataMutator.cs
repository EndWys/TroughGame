using Domain;
using ProjectCore.GameCore;

namespace ProjectCore.GameCore
{
    public interface IMovementStateDataMutator :
        IMovementStateDataAccessor,
        IStateDataMutator<MovementStates>
    {
        public void ChangeMovementState(MovementStates newState);
    }
}
