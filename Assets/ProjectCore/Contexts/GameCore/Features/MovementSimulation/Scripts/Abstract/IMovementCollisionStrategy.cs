using UnityEngine;

namespace ProjectCore.GameCore
{
    public interface IMovementCollisionStrategy
    {
        public Vector2 ResolveDisplacement(
            IMovementCollisionBodyAccessor body,
            Vector2 desiredDisplacement);
    }
}
