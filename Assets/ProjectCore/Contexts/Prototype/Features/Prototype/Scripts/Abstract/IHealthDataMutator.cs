namespace ProjectCore.Prototype
{
    public interface IHealthDataMutator : IHealthDataAccessor
    {
        public void ReduceHealth(byte amount);
        public void RestoreHealth(byte amount);
    }
}
