namespace ProjectCore.GameCore
{
    public readonly struct DodgeTimingData
    {
        public DodgeTimingData(int preparationTicks, int activeTicks, int recoveryTicks)
        {
            PreparationTicks = preparationTicks;
            ActiveTicks = activeTicks;
            RecoveryTicks = recoveryTicks;
        }

        public int PreparationTicks { get; }
        public int ActiveTicks { get; }
        public int RecoveryTicks { get; }
        public int TotalTicks => PreparationTicks + ActiveTicks + RecoveryTicks;
    }
}
