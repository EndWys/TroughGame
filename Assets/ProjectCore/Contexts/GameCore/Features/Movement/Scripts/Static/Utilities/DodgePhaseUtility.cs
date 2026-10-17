namespace ProjectCore.GameCore
{
    public static class DodgePhaseUtility
    {
        public static DodgeMovementPhase Evaluate(in DodgeTimingData timing, int? endTick, int currentTick)
        {
            if (!endTick.HasValue) return DodgeMovementPhase.None;
            int remaining = unchecked(endTick.Value - currentTick);
            if (remaining <= 0) return DodgeMovementPhase.None;
            int elapsed = unchecked(timing.TotalTicks - remaining);
            if (elapsed < timing.PreparationTicks) return DodgeMovementPhase.Preparation;
            if (elapsed < timing.PreparationTicks + timing.ActiveTicks) return DodgeMovementPhase.Active;
            return DodgeMovementPhase.Recovery;
        }
    }
}
