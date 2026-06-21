namespace Prototype.Prototype
{
    public interface IPlayerMovementStateProcessor
    {
        bool Execute(PlayerInputData input, out MovementStates resultState);
    }
}
