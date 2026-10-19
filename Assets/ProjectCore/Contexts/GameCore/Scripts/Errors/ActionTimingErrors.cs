namespace ProjectCore.GameCore
{
    public static class ActionTimingErrors
    {
        public const string PositiveDurationRequired = "A finite positive duration is required.";
        public const string InvalidRecoveryCancelWindow = "The cancellation window must lie within Recovery.";
        public const string DurationExceedsTickRange = "The duration exceeds the tick range.";
        public const string TotalDurationExceedsTickRange = "The action duration must be below half the tick-counter range.";
        public const string CollapsedRecoveryCancelWindow = "The authored cancellation window collapses at the simulation tick rate.";
    }
}
