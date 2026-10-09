using UnityEngine;

namespace ProjectCore.GameCore
{
    public interface IMovementBodyAccessor :
        IMovementCollisionBodyAccessor,
        IMovementBodyVelocityAccessor
    {
    }
}
