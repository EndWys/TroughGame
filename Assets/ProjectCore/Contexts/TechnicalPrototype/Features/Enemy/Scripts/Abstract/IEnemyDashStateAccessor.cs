using Fusion;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public interface IEnemyDashStateAccessor : IMovementStateTimerAccessor
    {
        Vector2 DashTargetPosition { get; }
        Vector2 DashDirection { get; }
        bool IsDashActionFinished { get; }
        bool IsDashCooldownFinished { get; }
        TickTimer DashActionTimer { get; }
        TickTimer DashCooldownTimer { get; }
    }
}
