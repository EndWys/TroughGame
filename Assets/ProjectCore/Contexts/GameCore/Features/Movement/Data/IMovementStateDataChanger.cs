using Domain;

namespace Prototype.Prototype
{
    public interface IMovementStateDataChanger :
        IMovementStateDataAccessor,
        IStateDataChanger<MovementStates>
    {
        public void ChangeMovementState(MovementStates newState);
    }
}
