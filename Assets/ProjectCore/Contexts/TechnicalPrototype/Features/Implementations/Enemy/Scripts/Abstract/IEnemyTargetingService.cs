using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public interface IEnemyTargetingService
    {
        bool HasAvailableTarget();

        bool IsTargetAvailable(NetworkEntityIdData targetEntityId);

        bool TryGetTargetPosition(NetworkEntityIdData targetEntityId, out Vector2 targetPosition);

        bool TryFindNearestTarget(Vector2 origin, out NetworkEntityIdData targetEntityId);
    }
}
