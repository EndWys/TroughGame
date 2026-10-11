namespace ProjectCore.Prototype
{
    public interface IPoseDataMutator : IPoseDataAccessor
    {
        public void ChangePose(PoseTypes newPose);
    }
}
