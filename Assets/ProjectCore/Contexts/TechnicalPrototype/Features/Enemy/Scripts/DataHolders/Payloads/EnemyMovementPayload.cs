using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public readonly struct EnemyMovementPayload : ILocomotionPayload
    {
        public EnemyMovementPayload(
            Vector2 direction,
            bool isDashRequested,
            Vector2 dashTargetPosition)
        {
            Direction = direction;
            IsDashRequested = isDashRequested;
            DashTargetPosition = dashTargetPosition;
        }

        public Vector2 Direction { get; }
        public bool IsDashRequested { get; }
        public Vector2 DashTargetPosition { get; }
    }
}
