namespace GameCore.Movement
{
    public interface IPoseDataChanger : IPoseDataAccessor
    {
        public void ChangePose(PoseTypes newPose);
    }
}