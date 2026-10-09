namespace ProjectCore.Template
{
    internal readonly struct DebugConsolePerformanceData
    {
        public DebugConsolePerformanceData(
            float framesPerSecond,
            float frameTimeMilliseconds,
            float monoUsedMegabytes,
            float totalReservedMegabytes,
            float gcHeapMegabytes)
        {
            FramesPerSecond = framesPerSecond;
            FrameTimeMilliseconds = frameTimeMilliseconds;
            MonoUsedMegabytes = monoUsedMegabytes;
            TotalReservedMegabytes = totalReservedMegabytes;
            GCHeapMegabytes = gcHeapMegabytes;
        }

        public float FramesPerSecond { get; }
        public float FrameTimeMilliseconds { get; }
        public float MonoUsedMegabytes { get; }
        public float TotalReservedMegabytes { get; }
        public float GCHeapMegabytes { get; }
    }
}
