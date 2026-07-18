using Domain;

namespace GameCore.Movement
{
    public interface IMovementStateDataChanger :
        IMovementStateDataAccessor,
        IStateDataChanger<MovementStates>
    {
        public void ChangeMovementState(MovementStates newState);
    }
}
