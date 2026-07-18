using Fusion;

namespace GameCore.Movement
{
    public interface IPoseDataAccessor
    {
        public PoseTypes CurrentPose { get; }
    }
}