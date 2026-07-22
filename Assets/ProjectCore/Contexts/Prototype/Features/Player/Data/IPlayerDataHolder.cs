using ProjectCore.GameCore;

namespace ProjectCore.Prototype
{
    public interface IPlayerDataHolder :
        IClimbDetectorDataMutator,
        IPoseDataMutator,
        IGroundDetectorDataMutator,
        IHealthDataMutator,
        IJumpDataMutator,
        IMovementStateDataMutator
    {
    }
}
