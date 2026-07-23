namespace ProjectCore.GameCore
{
    public interface IPoseDataMutator : IPoseDataAccessor
    {
        public void ChangePose(PoseTypes newPose);
    }
}
