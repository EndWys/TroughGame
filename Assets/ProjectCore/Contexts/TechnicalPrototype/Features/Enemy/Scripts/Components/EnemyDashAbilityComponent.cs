using Shared;
using ProjectCore.Template;
using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyDashAbilityComponent :
        MonoBehaviour,
        IEnemyEntityStateContributor,
        IEnemyChaseAbilityContributor
    {
        [SerializeField] private EnemyDashState _dashState;
        [SerializeField] private EnemyDashAbilityConfig _dashAbilityConfig;

        private IClassFactory _classFactory;

        [Inject]
        private void Construct(IClassFactory classFactory)
        {
            _classFactory = classFactory ?? throw new ArgumentNullException(nameof(classFactory));
        }

        public void AddEntityStates(
            IDictionary<EnemyEntityState, BaseEntityState<EnemyEntityState, EnemyEntityPayload>>
                entityStates)
        {
            if (entityStates == null)
            {
                throw new ArgumentNullException(nameof(entityStates));
            }

            entityStates.Add(EnemyEntityState.Dash, RequireDashState());
        }

        public void AddTransitionProcessors(
            EnemyEntityState sourceState,
            IList<IStateProcessor<EnemyEntityState, EnemyEntityPayload>> processors)
        {
            if (processors == null)
            {
                throw new ArgumentNullException(nameof(processors));
            }

            if (sourceState != EnemyEntityState.Idle &&
                sourceState != EnemyEntityState.Locomotion)
            {
                return;
            }

            processors.Add(_classFactory.Create<EnemyDashTransitionProcessor>(new object[] { RequireDashAbilityConfig().DashMovementConfig }));
        }

        public BaseEnemyChaseAbilityProcessor CreateChaseAbilityProcessor()
        {
            return RequireDashAbilityConfig().CreateChaseAbilityProcessor(
                _classFactory);
        }

        private EnemyDashState RequireDashState()
        {
            return _dashState != null
                ? _dashState
                : throw new InvalidOperationException("Enemy dash ability requires a dash state reference.");
        }

        private EnemyDashAbilityConfig RequireDashAbilityConfig()
        {
            return _dashAbilityConfig != null
                ? _dashAbilityConfig
                : throw new InvalidOperationException("Enemy dash ability requires a config reference.");
        }
    }
}
