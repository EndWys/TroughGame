using Fusion;

namespace ProjectCore.GameCore
{
    public interface IInputBufferStateAccessor
    {
        InputBufferCommandData BufferedCommand { get; }

        TickTimer BufferedCommandTimer { get; }

        bool IsLocked { get; }
    }
}
