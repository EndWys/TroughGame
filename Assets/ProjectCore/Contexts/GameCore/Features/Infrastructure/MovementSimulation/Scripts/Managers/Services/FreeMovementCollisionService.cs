using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class FreeMovementCollisionService : IMovementCollisionStrategy
    {
        public Vector2 ResolveDisplacement(
            IMovementCollisionBodyAccessor body,
            Vector2 desiredDisplacement)
        {
            return desiredDisplacement;
        }
    }
}
