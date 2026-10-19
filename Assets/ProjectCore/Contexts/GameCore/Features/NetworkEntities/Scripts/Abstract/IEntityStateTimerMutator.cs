namespace ProjectCore.GameCore
{
    public interface IEntityStateTimerMutator : IEntityStateTimerAccessor
    {
        void StartStateTimerTicks(int durationTicks);
        void StopStateTimer();
    }
}
