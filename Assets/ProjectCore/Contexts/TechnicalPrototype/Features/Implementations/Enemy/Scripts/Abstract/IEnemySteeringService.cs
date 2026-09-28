using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public interface IEnemySteeringService
    {
        Vector2 ApproachPosition { get; }

        Vector2 CalculateDirection(
            NetworkEntityIdData targetEntityId,
            Vector2 targetPosition,
            float stoppingDistance,
            float separationDistance,
            float separationWeight);
    }
}
