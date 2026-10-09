using UnityEngine;

namespace ProjectCore.GameCore
{
    public interface IMovementBodyVelocityMutator : IMovementBodyVelocityAccessor
    {
        public new Vector2 Velocity { get; set; }
    }
}
