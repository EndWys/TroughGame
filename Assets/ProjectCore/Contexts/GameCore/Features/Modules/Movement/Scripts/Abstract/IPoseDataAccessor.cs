using Fusion;

namespace ProjectCore.GameCore
{
    public interface IPoseDataAccessor
    {
        public PoseTypes CurrentPose { get; }
    }
}
