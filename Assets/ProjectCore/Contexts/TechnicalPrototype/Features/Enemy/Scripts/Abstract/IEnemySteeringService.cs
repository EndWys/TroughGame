using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public interface IEnemySteeringService
    {
        // Debug visualization reads the last calculation; gameplay uses the method's return value.
        EnemySteeringData LastCalculation { get; }

        EnemySteeringData CalculateSteering(
            NetworkEntityIdData targetEntityId,
            Vector2 targetPosition,
            float stoppingDistance,
            float separationDistance,
            float separationWeight);
    }
}
