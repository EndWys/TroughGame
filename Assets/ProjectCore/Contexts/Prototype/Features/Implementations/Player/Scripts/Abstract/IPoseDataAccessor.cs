using Fusion;

namespace ProjectCore.Prototype
{
    public interface IPoseDataAccessor
    {
        public PoseTypes CurrentPose { get; }
    }
}
