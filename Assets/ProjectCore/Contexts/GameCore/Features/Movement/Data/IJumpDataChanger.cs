namespace GameCore.Movement
{
    public interface IJumpDataChanger : IJumpDataAccessor
    {
        public void ChangeJumpingStatus(bool isJumping);
        public void RestartCoyoteTimer(int ticks);
        public void RestartJumpBufferTimer(int ticks);
        public void StopCoyoteTimer();
        public void StopJumpBufferTimer();
    }
}