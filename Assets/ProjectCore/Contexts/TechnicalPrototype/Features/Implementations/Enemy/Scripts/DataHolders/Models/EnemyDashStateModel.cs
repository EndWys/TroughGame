using Fusion;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public struct EnemyDashStateModel : INetworkStruct
    {
        public Vector2 TargetPosition;
        public Vector2 Direction;
        public TickTimer ActionTimer;
        public TickTimer CooldownTimer;
    }
}
