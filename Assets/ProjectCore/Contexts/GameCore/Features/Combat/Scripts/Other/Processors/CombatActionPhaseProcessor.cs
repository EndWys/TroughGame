using System;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class CombatActionPhaseProcessor
    {
        public void Start(
            ref CombatActionStateModel state,
            ushort actionId,
            Vector2 direction,
            int currentTick)
        {
            if (actionId == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(actionId));
            }

            if (float.IsNaN(direction.x) || float.IsInfinity(direction.x) ||
                float.IsNaN(direction.y) || float.IsInfinity(direction.y))
            {
                throw new ArgumentOutOfRangeException(nameof(direction));
            }

            uint executionId = unchecked(state.ExecutionId + 1);
            state = new CombatActionStateModel
            {
                ActionId = actionId,
                ExecutionId = executionId == 0 ? 1u : executionId,
                StartedTick = currentTick,
                Direction = direction.normalized,
            };
        }

        public CombatActionSnapshotData Evaluate(
            in CombatActionStateModel state,
            CombatActionKind kind,
            in ActionTimingData timing,
            int currentTick)
        {
            ActionTimelineData timeline = ActionTimelineData.FromStartTick(
                timing, state.ActionId != 0 ? state.StartedTick : (int?)null);
            ActionPhaseSnapshotData snapshot = timeline.Evaluate(currentTick);

            return new CombatActionSnapshotData(
                state.ActionId,
                state.ExecutionId,
                kind,
                snapshot.Phase,
                state.Direction,
                snapshot.IsRecoveryCancelWindowOpen,
                snapshot.IsComplete);
        }

        public void Cancel(ref CombatActionStateModel state)
        {
            state = new CombatActionStateModel { ExecutionId = state.ExecutionId };
        }
    }
}
