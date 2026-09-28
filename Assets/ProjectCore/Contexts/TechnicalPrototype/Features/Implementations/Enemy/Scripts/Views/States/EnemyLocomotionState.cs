using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyLocomotionState :
        BaseMovementState<EnemyMovementState, EnemyMovementPayload>
    {
        [SerializeField] private LocomotionMovementConfig _locomotionMovementConfig;
        [SerializeField] private EnemyChaseConfig _chaseConfig;
        [SerializeField] private TransformMovementBodyComponent _movementBody;

        private IMovementObstacleProbe _movementObstacleProbe;

        [Inject]
        private void Construct(IMovementObstacleProbe movementObstacleProbe)
        {
            _movementObstacleProbe = movementObstacleProbe ??
                throw new ArgumentNullException(nameof(movementObstacleProbe));
        }

        public override void Enter() { }

        public override void Exit() { }

        protected override IReadOnlyList<IMovementStateProcessor<EnemyMovementState, EnemyMovementPayload>>
            CreateMovementProcessors()
        {
            if (_movementBody == null)
            {
                throw new InvalidOperationException("Enemy locomotion state requires a movement body reference.");
            }

            if (_locomotionMovementConfig == null)
            {
                throw new InvalidOperationException(
                    "Enemy locomotion state requires a locomotion movement config reference.");
            }

            if (_chaseConfig == null)
            {
                throw new InvalidOperationException("Enemy locomotion state requires a chase config reference.");
            }

            return new IMovementStateProcessor<EnemyMovementState, EnemyMovementPayload>[]
            {
                new ObstacleAvoidanceLocomotionProcessor<EnemyMovementState, EnemyMovementPayload>(
                    movementBodyVelocityMutator: _movementBody,
                    movementCollisionBodyAccessor: _movementBody,
                    movementObstacleProbe: _movementObstacleProbe,
                    locomotionMovementConfig: _locomotionMovementConfig,
                    obstacleProbeDistance: _chaseConfig.ObstacleProbeDistance),
                new NoDirectionTransitionProcessor<EnemyMovementState, EnemyMovementPayload>(
                    idleMovementState: EnemyMovementState.Idle),
            };
        }

        protected override EnemyMovementState FallbackState => EnemyMovementState.Locomotion;
    }
}
