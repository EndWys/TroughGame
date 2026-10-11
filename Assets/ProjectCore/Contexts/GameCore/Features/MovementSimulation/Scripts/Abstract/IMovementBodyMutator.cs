using UnityEngine;

namespace ProjectCore.GameCore
{
    public interface IMovementBodyMutator :
        IMovementBodyAccessor,
        IMovementBodyVelocityMutator
    {
        public void ApplySimulation(
            Vector2 resolvedVelocity,
            Vector2 resolvedDisplacement);
    }
}
