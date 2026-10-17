namespace ProjectCore.GameCore
{
    public readonly struct LocalButtonStateData
    {
        public LocalButtonStateData(
            bool isHeld,
            bool wasPressed,
            bool wasReleased,
            bool wasPerformed)
            : this(isHeld, wasPressed, wasReleased, wasPerformed, 0)
        {
        }

        public LocalButtonStateData(
            bool isHeld,
            bool wasPressed,
            bool wasReleased,
            bool wasPerformed,
            ulong pressSequence)
        {
            IsHeld = isHeld;
            WasPressed = wasPressed;
            WasReleased = wasReleased;
            WasPerformed = wasPerformed;
            PressSequence = pressSequence;
        }

        public bool IsHeld { get; }
        public bool WasPressed { get; }
        public bool WasReleased { get; }
        public bool WasPerformed { get; }
        public ulong PressSequence { get; }
    }
}
