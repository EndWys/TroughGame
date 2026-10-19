using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public readonly struct PlayerInputSnapshotData
    {
        public PlayerInputSnapshotData(Vector2 moveDirection, Vector2 lastNonZeroDirection)
        {
            MoveDirection = moveDirection;
            LastNonZeroDirection = lastNonZeroDirection;
        }

        public Vector2 MoveDirection { get; }
        public Vector2 LastNonZeroDirection { get; }
    }
}
