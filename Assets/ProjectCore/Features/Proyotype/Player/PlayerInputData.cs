using Fusion;

namespace ProjectCore.Features.Prototype.Player
{
    public struct PlayerInputData : INetworkInput
    {
        public float Horizontal { get; set; }
        public float Vertical { get; set; }
    }
}