using Shared;
using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyIdleState : BaseEntityState<EnemyEntityState, EnemyEntityPayload>
    {
        private IEnemyAbilityCollection _abilityCollection;

        [Inject]
        private void Construct(IEnemyAbilityCollection abilityCollection)
        {
            _abilityCollection = abilityCollection ??
                throw new ArgumentNullException(nameof(abilityCollection));
        }

        public override void Enter()
        {
        }

        public override void Exit()
        {
        }

        protected override IReadOnlyList<IStateProcessor<EnemyEntityState, EnemyEntityPayload>>
            CreateProcessors()
        {
            var processors = new List<IStateProcessor<EnemyEntityState, EnemyEntityPayload>>();

            foreach (IEnemyEntityStateContributor ability in
                     _abilityCollection.GetAbilities<IEnemyEntityStateContributor>())
            {
                ability.AddTransitionProcessors(EnemyEntityState.Idle, processors);
            }

            processors.Add(
                CreateProcessor<DirectionalLocomotionTransitionProcessor<EnemyEntityState, EnemyEntityPayload>>(
                    EnemyEntityState.Locomotion));

            return processors;
        }

        protected override EnemyEntityState FallbackState => EnemyEntityState.Idle;
    }
}
