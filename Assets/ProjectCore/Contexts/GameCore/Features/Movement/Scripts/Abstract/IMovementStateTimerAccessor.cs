namespace ProjectCore.GameCore
{
    public interface IMovementStateTimerAccessor
    {
        int? StateTimerEndTick { get; }

        bool IsStateTimerFinished { get; }
    }
}
