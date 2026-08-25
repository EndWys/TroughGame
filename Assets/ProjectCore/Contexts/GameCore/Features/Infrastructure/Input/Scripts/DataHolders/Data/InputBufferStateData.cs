using Fusion;

namespace ProjectCore.GameCore
{
    public struct InputBufferStateData : INetworkStruct
    {
        public InputBufferCommandData BufferedCommand;
        public TickTimer BufferedCommandTimer;
        public NetworkBool IsLocked;
    }
}
