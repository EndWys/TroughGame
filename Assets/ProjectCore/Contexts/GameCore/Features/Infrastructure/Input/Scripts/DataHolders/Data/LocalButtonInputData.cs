namespace ProjectCore.GameCore
{
    public readonly struct LocalButtonInputData
    {
        public LocalButtonInputData(
            bool isHeld,
            bool wasPressed,
            bool wasReleased,
            bool wasPerformed)
        {
            IsHeld = isHeld;
            WasPressed = wasPressed;
            WasReleased = wasReleased;
            WasPerformed = wasPerformed;
        }

        public bool IsHeld { get; }
        public bool WasPressed { get; }
        public bool WasReleased { get; }
        public bool WasPerformed { get; }
    }
}
