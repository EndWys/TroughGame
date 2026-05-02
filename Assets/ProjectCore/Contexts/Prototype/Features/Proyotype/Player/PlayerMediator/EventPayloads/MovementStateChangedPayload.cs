namespace Prototype.Prototype
{
    public struct MovementStateChangedPayload
    {
        public MovementStates MovementStates { get; set; }
        public MovementStates PreviousMovementStates { get; set; }
    }
}