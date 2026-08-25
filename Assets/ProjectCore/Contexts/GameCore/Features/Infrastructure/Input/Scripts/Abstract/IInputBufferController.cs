namespace ProjectCore.GameCore
{
    public interface IInputBufferController
    {
        bool IsLocked { get; }

        bool HasBufferedCommand(ushort commandId);

        void BufferCommand(InputBufferCommandDescriptor command, float lifetimeSeconds);

        bool BeginAction(ushort commandId);

        void EndAction();
    }
}
