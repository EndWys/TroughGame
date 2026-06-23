namespace Prototype.Prototype
{
    public interface IHealthDataChanger : IHealthDataAccessor
    {
        public void ReduceHealth(byte amount);
        public void RestoreHealth(byte amount);
    }
}