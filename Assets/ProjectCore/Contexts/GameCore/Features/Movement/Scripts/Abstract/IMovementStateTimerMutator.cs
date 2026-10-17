namespace ProjectCore.GameCore
{
    public interface IMovementStateTimerMutator : IMovementStateTimerAccessor
    {
        void StartStateTimerTicks(int durationTicks);

        void StopStateTimer();
    }
}
