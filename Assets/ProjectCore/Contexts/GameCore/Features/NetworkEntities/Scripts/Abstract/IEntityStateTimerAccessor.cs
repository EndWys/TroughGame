namespace ProjectCore.GameCore
{
    public interface IEntityStateTimerAccessor
    {
        int? StateTimerEndTick { get; }
        bool IsStateTimerFinished { get; }
    }
}
