namespace Prototype.Prototype
{
    public interface IPoseDataChanger : IPoseDataAccessor
    {
        public void ChangePose(PoseTypes newPose);
    }
}