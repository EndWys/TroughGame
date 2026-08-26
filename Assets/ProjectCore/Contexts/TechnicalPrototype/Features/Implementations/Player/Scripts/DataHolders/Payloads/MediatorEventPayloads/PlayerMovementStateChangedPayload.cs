using Domain;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerMovementStateChangedPayload : HandlerPayload
    {
        public PlayerMovementStateChangedPayload(
            IColleague sender,
            PlayerMovementState currentMovementState,
            PlayerMovementState previousMovementState)
        {
            Sender = sender;
            CurrentMovementState = currentMovementState;
            PreviousMovementState = previousMovementState;
        }

        public PlayerMovementState CurrentMovementState { get; }
        public PlayerMovementState PreviousMovementState { get; }
    }
}
