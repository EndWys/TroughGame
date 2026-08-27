using Fusion;

namespace ProjectCore.GameCore
{
    public interface IInputBufferStateAccessor
    {
        InputBufferCommandDescriptor BufferedCommand { get; }

        TickTimer BufferedCommandTimer { get; }

        bool IsLocked { get; }
    }
}
