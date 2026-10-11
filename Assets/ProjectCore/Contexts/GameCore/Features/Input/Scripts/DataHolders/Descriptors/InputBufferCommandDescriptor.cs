namespace ProjectCore.GameCore
{
    public readonly struct InputBufferCommandDescriptor
    {
        private readonly ushort _commandId;

        public InputBufferCommandDescriptor(ushort commandId)
        {
            _commandId = commandId;
        }

        public ushort CommandId => _commandId;
    }
}
