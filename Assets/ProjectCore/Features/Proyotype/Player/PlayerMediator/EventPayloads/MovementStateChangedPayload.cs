namespace ProjectCore.Features.Prototype.Player.PlayerMediator.EventPayloads
{
    public struct MovementStateChangedPayload
    {
        public EMovementState MovementState { get; set; }
        public EMovementState PreviousMovementState { get; set; }
    }
}