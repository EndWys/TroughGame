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
        private IEnemyAbilityCollection _abilityCollection;

        [Inject]
        private void Construct(
            IMovementObstacleProbe movementObstacleProbe,
            IEnemyAbilityCollection abilityCollection)
        {
            _movementObstacleProbe = movementObstacleProbe ??
                throw new ArgumentNullException(nameof(movementObstacleProbe));
            _abilityCollection = abilityCollection ??
                throw new ArgumentNullException(nameof(abilityCollection));
        }

        public override void Enter() { }

        public override void Exit() { }

        protected override IReadOnlyList<IMovementStateProcessor<EnemyMovementState, EnemyMovementPayload>>
            CreateMovementProcessors()
        {
            if (_movementBody == null)
            {
                throw new InvalidOperationException(
                    "Enemy locomotion state requires a movement body reference.");
            }

            if (_locomotionMovementConfig == null)
            {
                throw new InvalidOperationException(
                    "Enemy locomotion state requires a locomotion movement config reference.");
            }

            if (_chaseConfig == null)
            {
                throw new InvalidOperationException(
                    "Enemy locomotion state requires a chase config reference.");
            }

            var processors = new List<IMovementStateProcessor<EnemyMovementState, EnemyMovementPayload>>();

            foreach (IEnemyMovementAbilityContributor ability in
                     _abilityCollection.GetAbilities<IEnemyMovementAbilityContributor>())
            {
                ability.AddTransitionProcessors(EnemyMovementState.Locomotion, processors);
            }

            processors.Add(new ObstacleAvoidanceLocomotionProcessor<EnemyMovementState, EnemyMovementPayload>(
                movementBodyVelocityMutator: _movementBody,
                movementCollisionBodyAccessor: _movementBody,
                movementObstacleProbe: _movementObstacleProbe,
                locomotionMovementConfig: _locomotionMovementConfig,
                obstacleProbeDistance: _chaseConfig.ObstacleProbeDistance));
            processors.Add(new NoDirectionTransitionProcessor<EnemyMovementState, EnemyMovementPayload>(
                idleMovementState: EnemyMovementState.Idle));
            return processors;
        }

        protected override EnemyMovementState FallbackState => EnemyMovementState.Locomotion;
    }
}
