using Fusion;

namespace ProjectCore.GameCore
{
    public interface IInputBufferStateMutator : IInputBufferStateAccessor
    {
        void SetBufferedCommand(
            InputBufferCommandData command,
            TickTimer expirationTimer);

        void ClearBufferedCommand();

        void LockInput();

        void UnlockInput();
    }
}
