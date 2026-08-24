using Fusion;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public struct PlayerInputData : INetworkInput
    {
        public Vector2 MoveDirection { get; set; }
    }
}
