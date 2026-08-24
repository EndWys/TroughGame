using Fusion;

namespace ProjectCore.GameCore
{
    public interface IBufferedInputData
    {
        int Sequence { get; }
        Tick ExpireTick { get; }
    }
}
