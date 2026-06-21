namespace Prototype.Prototype
{
    public interface IPlayerDataHolder :
        IClimbDetectorDataChanger,
        IPoseDataChanger,
        IGroundDetectorDataChanger,
        IHealthDataChanger,
        IJumpDataChanger,
        IMovementStateDataChanger<MovementStates>
    {
    }
}
