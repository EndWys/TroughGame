using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public readonly struct EnemyInputData
    {
        public EnemyInputData(Vector2 direction)
        {
            Direction = direction;
        }

        public Vector2 Direction { get; }
    }
}
