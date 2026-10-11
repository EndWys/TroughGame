using UnityEngine;

namespace ProjectCore.GameCore
{
    public interface IMovementBodyVelocityAccessor
    {
        public Vector2 Velocity { get; }
    }
}
