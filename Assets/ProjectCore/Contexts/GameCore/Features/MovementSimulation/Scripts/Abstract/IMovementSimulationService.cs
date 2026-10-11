namespace ProjectCore.GameCore
{
    public interface IMovementSimulationService
    {
        public void Simulate(IMovementBodyMutator body, float deltaTime);
    }
}
