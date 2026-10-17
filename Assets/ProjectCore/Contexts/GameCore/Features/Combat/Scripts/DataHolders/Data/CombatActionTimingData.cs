namespace ProjectCore.GameCore
{
    public readonly struct CombatActionTimingData
    {
        public CombatActionTimingData(
            int preparationTicks,
            int activeTicks,
            int recoveryTicks,
            int comboWindowStartTicks,
            int comboWindowEndTicks)
        {
            PreparationTicks = preparationTicks;
            ActiveTicks = activeTicks;
            RecoveryTicks = recoveryTicks;
            ComboWindowStartTicks = comboWindowStartTicks;
            ComboWindowEndTicks = comboWindowEndTicks;
        }

        public int PreparationTicks { get; }
        public int ActiveTicks { get; }
        public int RecoveryTicks { get; }
        public int ComboWindowStartTicks { get; }
        public int ComboWindowEndTicks { get; }
    }
}
