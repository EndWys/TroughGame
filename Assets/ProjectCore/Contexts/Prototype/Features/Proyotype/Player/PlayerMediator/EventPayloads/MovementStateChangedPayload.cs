using Domain;

namespace Prototype.Prototype
{
    public class MovementStateChangedPayload : HandlerPayload
    {
        public MovementStates MovementStates { get; set; }
        public MovementStates PreviousMovementStates { get; set; }
    }
}
