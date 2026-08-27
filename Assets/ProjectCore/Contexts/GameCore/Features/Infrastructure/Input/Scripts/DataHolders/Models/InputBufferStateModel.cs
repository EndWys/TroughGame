using Fusion;

namespace ProjectCore.GameCore
{
    public struct InputBufferStateModel : INetworkStruct
    {
        // Command id 0 is reserved for an empty buffer slot.
        public ushort BufferedCommandId;
        public TickTimer BufferedCommandTimer;
        public NetworkBool IsLocked;
    }
}
