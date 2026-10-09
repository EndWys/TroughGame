using ProjectCore.GameCore;

namespace ProjectCore.Prototype
{
    public interface IPlayerDataMutator :
        IClimbDetectorDataMutator,
        IPoseDataMutator,
        IGroundDetectorDataMutator,
        IHealthDataMutator,
        IJumpDataMutator,
        IMovementStateDataMutator<MovementStates>
    {
    }
}
