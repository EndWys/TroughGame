namespace ProjectCore.GameCore
{
    public readonly struct ActionTimelineData
    {
        private readonly ActionTimingData _timing;

        private ActionTimelineData(in ActionTimingData timing, int startedTick)
        {
            _timing = timing;
            StartedTick = startedTick;
        }

        public int? StartedTick { get; }
        public int? ActiveStartTick => OffsetTick(_timing.PreparationTicks);
        public int? ActiveEndTick => RecoveryStartTick;
        public int? RecoveryStartTick => OffsetTick(_timing.PreparationTicks + _timing.ActiveTicks);
        public int? EndTick => OffsetTick(_timing.TotalTicks);
        public int? RecoveryCancelWindowStartTick => _timing.HasRecoveryCancelWindow
            ? OffsetTick(_timing.PreparationTicks + _timing.ActiveTicks + _timing.RecoveryCancelWindowStartTicks)
            : null;
        public int? RecoveryCancelWindowEndTick => _timing.HasRecoveryCancelWindow
            ? OffsetTick(_timing.PreparationTicks + _timing.ActiveTicks + _timing.RecoveryCancelWindowEndTicks)
            : null;

        public static ActionTimelineData FromStartTick(in ActionTimingData timing, int? startedTick)
        {
            return startedTick.HasValue ? new ActionTimelineData(timing, startedTick.Value) : default;
        }

        public static ActionTimelineData FromEndTick(in ActionTimingData timing, int? endTick)
        {
            return endTick.HasValue
                ? new ActionTimelineData(timing, unchecked(endTick.Value - timing.TotalTicks))
                : default;
        }

        public ActionPhaseSnapshotData Evaluate(int currentTick)
        {
            if (!StartedTick.HasValue) return default;

            int elapsed = unchecked(currentTick - StartedTick.Value);
            if (elapsed < 0) return default;
            if (elapsed >= _timing.TotalTicks)
                return new ActionPhaseSnapshotData(ActionPhase.None, true, false);
            if (elapsed < _timing.PreparationTicks)
                return new ActionPhaseSnapshotData(ActionPhase.Preparation, false, false);

            int recoveryStart = _timing.PreparationTicks + _timing.ActiveTicks;
            if (elapsed < recoveryStart)
                return new ActionPhaseSnapshotData(ActionPhase.Active, false, false);

            int recoveryElapsed = elapsed - recoveryStart;
            bool isWindowOpen = recoveryElapsed >= _timing.RecoveryCancelWindowStartTicks &&
                recoveryElapsed < _timing.RecoveryCancelWindowEndTicks;
            return new ActionPhaseSnapshotData(ActionPhase.Recovery, false, isWindowOpen);
        }

        private int? OffsetTick(int offset)
        {
            return StartedTick.HasValue ? unchecked(StartedTick.Value + offset) : null;
        }
    }
}
