using Fusion;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public struct CombatActionStateModel : INetworkStruct
    {
        // Zero identifies an idle actor; gameplay IDs are independent of input IDs.
        public ushort ActionId;
        public uint ExecutionId;
        public int StartedTick;
        public Vector2 Direction;
    }
}
