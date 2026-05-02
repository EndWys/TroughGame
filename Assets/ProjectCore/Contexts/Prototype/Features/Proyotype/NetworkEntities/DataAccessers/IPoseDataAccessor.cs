using Fusion;

namespace Prototype.Prototype
{
    public interface IPoseDataAccessor
    {
        public PoseTypes CurrentPose { get; }
    }
}