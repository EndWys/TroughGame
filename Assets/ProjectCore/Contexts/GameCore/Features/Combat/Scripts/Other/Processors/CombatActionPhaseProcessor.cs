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
            in CombatActionTimingData timing,
            int currentTick)
        {
            CombatActionPhase phase = CombatActionPhase.None;
            bool canStartNextAction = false;
            bool isComplete = false;

            if (state.ActionId != 0)
            {
                int elapsed = unchecked(currentTick - state.StartedTick);
                long recoveryStart = (long)timing.PreparationTicks + timing.ActiveTicks;
                long totalTicks = recoveryStart + timing.RecoveryTicks;

                if (elapsed >= totalTicks)
                {
                    isComplete = true;
                }
                else if (elapsed >= recoveryStart)
                {
                    phase = CombatActionPhase.Recovery;
                    long recoveryElapsed = elapsed - recoveryStart;
                    canStartNextAction = recoveryElapsed >= timing.ComboWindowStartTicks &&
                        recoveryElapsed < timing.ComboWindowEndTicks;
                }
                else if (elapsed >= timing.PreparationTicks)
                {
                    phase = CombatActionPhase.Active;
                }
                else if (elapsed >= 0)
                {
                    phase = CombatActionPhase.Preparation;
                }
            }

            return new CombatActionSnapshotData(
                state.ActionId,
                state.ExecutionId,
                kind,
                phase,
                state.Direction,
                canStartNextAction,
                isComplete);
        }

        public void Cancel(ref CombatActionStateModel state)
        {
            state = new CombatActionStateModel { ExecutionId = state.ExecutionId };
        }
    }
}
