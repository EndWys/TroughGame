namespace ProjectCore.GameCore
{
    public interface IMovementStateTimerMutator : IMovementStateTimerAccessor
    {
        void StartStateTimer(float durationSeconds);

        void StopStateTimer();
    }
}
