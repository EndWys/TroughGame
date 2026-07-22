using Fusion;

namespace ProjectCore.GameCore
{
    public interface IJumpDataAccessor
    {
        public bool IsJumping { get; }
        public TickTimer CoyoteTimer { get; }
        public TickTimer JumpBufferTimer { get; }
    }
}
