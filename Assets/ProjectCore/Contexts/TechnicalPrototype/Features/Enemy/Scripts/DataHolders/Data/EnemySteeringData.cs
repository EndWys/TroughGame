using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public readonly struct EnemySteeringData
    {
        public EnemySteeringData(Vector2 direction, Vector2 approachPosition)
        {
            Direction = direction;
            ApproachPosition = approachPosition;
        }

        public Vector2 Direction { get; }
        public Vector2 ApproachPosition { get; }
    }
}
