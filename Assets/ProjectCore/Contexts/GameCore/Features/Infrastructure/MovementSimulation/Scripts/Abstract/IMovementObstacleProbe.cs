using UnityEngine;

namespace ProjectCore.GameCore
{
    public interface IMovementObstacleProbe
    {
        bool TryProbe(
            IMovementCollisionBodyAccessor body,
            Vector2 direction,
            float distance,
            out Vector2 obstacleNormal);
    }
}
