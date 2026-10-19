using UnityEngine;

namespace ProjectCore.GameCore
{
    public readonly struct CombatActionSnapshotData
    {
        public CombatActionSnapshotData(
            ushort actionId,
            uint executionId,
            CombatActionKind kind,
            ActionPhase phase,
            Vector2 direction,
            bool canStartNextAction,
            bool isComplete)
        {
            ActionId = actionId;
            ExecutionId = executionId;
            Kind = kind;
            Phase = phase;
            Direction = direction;
            CanStartNextAction = canStartNextAction;
            IsComplete = isComplete;
        }

        public ushort ActionId { get; }
        public uint ExecutionId { get; }
        public CombatActionKind Kind { get; }
        public ActionPhase Phase { get; }
        public Vector2 Direction { get; }
        public bool CanStartNextAction { get; }
        public bool IsComplete { get; }
    }
}
