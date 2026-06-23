using Domain;

namespace Prototype.Prototype
{
    public class PlayerMovementStateChangedPayload : HandlerPayload
    {
        public MovementStates MovementStates { get; set; }
        public MovementStates PreviousMovementStates { get; set; }
    }
}
