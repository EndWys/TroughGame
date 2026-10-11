using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public readonly struct EnemyInputData
    {
        public EnemyInputData(
            Vector2 direction,
            bool isDashRequested = false,
            Vector2 dashTargetPosition = default)
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
