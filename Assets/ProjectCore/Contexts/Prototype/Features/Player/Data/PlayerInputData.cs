using Fusion;
using UnityEngine;

namespace Prototype.Prototype
{
    public struct PlayerInputData : INetworkInput
    {
        public Vector2 MoveDirection { get; set; }
        public float LookYawDelta { get; set; }
        public bool IsRunning { get; set; }
        public bool IsJumpPressed { get; set; }
        public bool IsCrouchPressed { get; set; }
    }
}