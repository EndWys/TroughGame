namespace ProjectCore.GameCore
{
    public readonly struct InputBufferSettings
    {
        public InputBufferSettings(
            InputBufferRepeatMode repeatMode,
            InputBufferOverflowMode overflowMode)
        {
            RepeatMode = repeatMode;
            OverflowMode = overflowMode;
        }

        public InputBufferRepeatMode RepeatMode { get; }
        public InputBufferOverflowMode OverflowMode { get; }
    }
}
