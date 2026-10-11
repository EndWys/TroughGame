using System;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class MovementSimulationService : IMovementSimulationService
    {
        private readonly IMovementCollisionStrategy _collisionStrategy;

        public MovementSimulationService(IMovementCollisionStrategy collisionStrategy)
        {
            _collisionStrategy = collisionStrategy ??
                throw new ArgumentNullException(nameof(collisionStrategy));
        }

        public void Simulate(IMovementBodyMutator body, float deltaTime)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (deltaTime <= 0f)
            {
                return;
            }

            Vector2 desiredDisplacement = body.Velocity * deltaTime;
            Vector2 resolvedDisplacement = _collisionStrategy.ResolveDisplacement(
                body,
                desiredDisplacement);
            Vector2 resolvedVelocity = desiredDisplacement.sqrMagnitude <= Mathf.Epsilon
                ? Vector2.zero
                : resolvedDisplacement / deltaTime;

            body.ApplySimulation(resolvedVelocity, resolvedDisplacement);
        }
    }
}
