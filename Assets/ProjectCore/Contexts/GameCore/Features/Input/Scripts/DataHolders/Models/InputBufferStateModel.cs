using System;
using Fusion;

namespace ProjectCore.GameCore
{
    public struct InputBufferStateModel : INetworkStruct
    {
        private byte _packedCommandAndLock;

        public TickTimer BufferedCommandTimer;

        // Zero is empty; input command IDs use the upper seven bits.
        public ushort BufferedCommandId
        {
            get => (ushort)(_packedCommandAndLock >> 1);
            set
            {
                if (value > 127)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _packedCommandAndLock =
                    (byte)((value << 1) | (_packedCommandAndLock & 1));
            }
        }

        public bool IsLocked
        {
            get => (_packedCommandAndLock & 1) != 0;
            set => _packedCommandAndLock =
                (byte)((_packedCommandAndLock & 0xFE) | (value ? 1 : 0));
        }
    }
}
