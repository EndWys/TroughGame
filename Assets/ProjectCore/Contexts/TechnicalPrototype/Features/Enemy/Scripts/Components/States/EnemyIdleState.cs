using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyIdleState : BaseMovementState<EnemyMovementState, EnemyMovementPayload>
    {
        private IEnemyAbilityCollection _abilityCollection;

        [Inject]
        private void Construct(IEnemyAbilityCollection abilityCollection)
        {
            _abilityCollection = abilityCollection ??
                throw new ArgumentNullException(nameof(abilityCollection));
        }

        public override void Enter() { }

        public override void Exit() { }

        protected override IReadOnlyList<IMovementStateProcessor<EnemyMovementState, EnemyMovementPayload>>
            CreateMovementProcessors()
        {
            var processors = new List<IMovementStateProcessor<EnemyMovementState, EnemyMovementPayload>>();

            foreach (IEnemyMovementAbilityContributor ability in
                     _abilityCollection.GetAbilities<IEnemyMovementAbilityContributor>())
            {
                ability.AddTransitionProcessors(EnemyMovementState.Idle, processors);
            }

            processors.Add(
                new DirectionalLocomotionTransitionProcessor<EnemyMovementState, EnemyMovementPayload>(
                    locomotionMovementState: EnemyMovementState.Locomotion));
            return processors;
        }

        protected override EnemyMovementState FallbackState => EnemyMovementState.Idle;
    }
}
