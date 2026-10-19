namespace ProjectCore.GameCore
{
    public readonly struct ActionPhaseSnapshotData
    {
        public ActionPhaseSnapshotData(ActionPhase phase, bool isComplete, bool isRecoveryCancelWindowOpen)
        {
            Phase = phase;
            IsComplete = isComplete;
            IsRecoveryCancelWindowOpen = isRecoveryCancelWindowOpen;
        }

        public ActionPhase Phase { get; }
        public bool IsComplete { get; }
        public bool IsRecoveryCancelWindowOpen { get; }
    }
}
