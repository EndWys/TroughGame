using Domain;
using ProjectCore.GameCore;

namespace ProjectCore.Prototype
{
    public class PlayerMovementStateChangedPayload : HandlerPayload
    {
        public MovementStates MovementStates { get; set; }
        public MovementStates PreviousMovementStates { get; set; }
    }
}
