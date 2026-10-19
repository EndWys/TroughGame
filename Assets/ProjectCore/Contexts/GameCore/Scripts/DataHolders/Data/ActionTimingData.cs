namespace ProjectCore.GameCore
{
    public readonly struct ActionTimingData
    {
        public ActionTimingData(
            int preparationTicks,
            int activeTicks,
            int recoveryTicks,
            int recoveryCancelWindowStartTicks = 0,
            int recoveryCancelWindowEndTicks = 0)
        {
            ActionTimingValidation.ValidateTicks(preparationTicks, activeTicks, recoveryTicks,
                recoveryCancelWindowStartTicks, recoveryCancelWindowEndTicks);
            PreparationTicks = preparationTicks;
            ActiveTicks = activeTicks;
            RecoveryTicks = recoveryTicks;
            RecoveryCancelWindowStartTicks = recoveryCancelWindowStartTicks;
            RecoveryCancelWindowEndTicks = recoveryCancelWindowEndTicks;
        }

        public int PreparationTicks { get; }
        public int ActiveTicks { get; }
        public int RecoveryTicks { get; }
        // Window offsets are measured from the beginning of Recovery.
        public int RecoveryCancelWindowStartTicks { get; }
        public int RecoveryCancelWindowEndTicks { get; }
        public bool HasRecoveryCancelWindow => RecoveryCancelWindowStartTicks < RecoveryCancelWindowEndTicks;
        public int TotalTicks => PreparationTicks + ActiveTicks + RecoveryTicks;
    }
}
