namespace Prototype.Prototype
{
    public struct MovementStateChangedPayload
    {
        public EMovementState MovementState { get; set; }
        public EMovementState PreviousMovementState { get; set; }
    }
}