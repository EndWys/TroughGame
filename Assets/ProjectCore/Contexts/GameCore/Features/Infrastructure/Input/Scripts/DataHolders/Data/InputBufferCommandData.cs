using Fusion;

namespace ProjectCore.GameCore
{
    public struct InputBufferCommandData : INetworkStruct
    {
        // CommandId 0 is reserved for an empty command slot.
        public ushort CommandId;
    }
}
