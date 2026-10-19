using Shared;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerEntityStateChangedPayload : HandlerPayload
    {
        public PlayerEntityStateChangedPayload(
            IColleague sender,
            PlayerEntityState currentMovementState,
            PlayerEntityState previousMovementState)
        {
            Sender = sender;
            CurrentEntityState = currentMovementState;
            PreviousEntityState = previousMovementState;
        }

        public PlayerEntityState CurrentEntityState { get; }
        public PlayerEntityState PreviousEntityState { get; }
    }
}
