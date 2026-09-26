using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public readonly struct EnemyInputFrameData
    {
        public EnemyInputFrameData(Vector2 direction)
        {
            Direction = direction;
        }

        public Vector2 Direction { get; }
    }
}
