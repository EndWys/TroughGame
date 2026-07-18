namespace GameCore.Movement
{
    public enum MovementStates : byte
    {
        Default = 0,
        
        Idle = 1,
        Walk = 2,
        Run = 3,
        Airborne = 4,
        Crouch = 5,
        Climb = 6,
    }
}