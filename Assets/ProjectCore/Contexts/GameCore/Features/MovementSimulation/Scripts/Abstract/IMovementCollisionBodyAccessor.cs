using UnityEngine;

namespace ProjectCore.GameCore
{
    public interface IMovementCollisionBodyAccessor
    {
        public Vector2 Position { get; }
        public float CollisionRadius { get; }
        public LayerMask CollisionMask { get; }
    }
}
