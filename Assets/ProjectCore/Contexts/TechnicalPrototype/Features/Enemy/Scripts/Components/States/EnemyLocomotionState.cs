using Shared;
using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyLocomotionState :
        BaseEntityState<EnemyEntityState, EnemyEntityPayload>
    {
        [SerializeField] private LocomotionMovementConfig _locomotionMovementConfig;
        [SerializeField] private EnemyChaseConfig _chaseConfig;

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

        protected override IReadOnlyList<IStateProcessor<EnemyEntityState, EnemyEntityPayload>>
            CreateProcessors()
        {
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

            var processors = new List<IStateProcessor<EnemyEntityState, EnemyEntityPayload>>();

            foreach (IEnemyEntityStateContributor ability in
                     _abilityCollection.GetAbilities<IEnemyEntityStateContributor>())
            {
                ability.AddTransitionProcessors(EnemyEntityState.Locomotion, processors);
            }

            processors.Add(CreateProcessor<ObstacleAvoidanceLocomotionProcessor<EnemyEntityState, EnemyEntityPayload>>(_locomotionMovementConfig, _chaseConfig.ObstacleProbeDistance));
            processors.Add(CreateProcessor<NoDirectionTransitionProcessor<EnemyEntityState, EnemyEntityPayload>>(EnemyEntityState.Idle));
            return processors;
        }

        protected override EnemyEntityState FallbackState => EnemyEntityState.Locomotion;
    }
}
