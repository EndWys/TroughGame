using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public interface IEnemyDashStateMutator : IEnemyDashStateAccessor
    {
        void StartDash(
            Vector2 targetPosition,
            Vector2 direction,
            float durationSeconds,
            float cooldownSeconds);

        void StopDash();
    }
}
