using Fusion;

namespace ProjectCore.Features.Proyotype
{
    public struct PlayerInputData : INetworkInput
    {
        public float Horizontal { get; set; }
        public float Vertical { get; set; }
    }
}