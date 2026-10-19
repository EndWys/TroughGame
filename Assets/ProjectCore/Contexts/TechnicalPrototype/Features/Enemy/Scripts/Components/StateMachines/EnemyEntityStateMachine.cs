using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "ProjectCore.TechnicalPrototype", null, "EnemyMovementStateMachine")]
    public sealed class EnemyEntityStateMachine : BaseEntityStateMachine<
        EnemyEntityState,
        BaseEntityState<EnemyEntityState, EnemyEntityPayload>,
        EnemyEntityPayload>
    {
        [SerializeField] private EnemyIdleState _idleState;
        [SerializeField] private EnemyLocomotionState _locomotionState;
        [SerializeField] private EnemyInputSourceComponent _inputSource;

        private IEnemyAbilityCollection _abilityCollection;

        [Inject]
        private void Construct(IEnemyAbilityCollection abilityCollection)
        {
            _abilityCollection = abilityCollection ??
                throw new System.ArgumentNullException(nameof(abilityCollection));
        }

        protected override EnemyEntityState InitialState => EnemyEntityState.Idle;

        public override Dictionary<
            EnemyEntityState,
            BaseEntityState<EnemyEntityState, EnemyEntityPayload>>
            CreateStatesDictionary()
        {
            var entityStates = new Dictionary<
                EnemyEntityState,
                BaseEntityState<EnemyEntityState, EnemyEntityPayload>>
            {
                { EnemyEntityState.Idle, _idleState },
                { EnemyEntityState.Locomotion, _locomotionState },
            };

            foreach (IEnemyEntityStateContributor ability in
                     _abilityCollection.GetAbilities<IEnemyEntityStateContributor>())
            {
                ability.AddEntityStates(entityStates);
            }

            return entityStates;
        }

        protected override bool TryGetPayload(out EnemyEntityPayload payload)
        {
            EnemyInputFrameData input = default;

            if (_inputSource != null)
            {
                _inputSource.TryGetInput(out input);
            }

            payload = new EnemyEntityPayload(
                direction: input.Direction,
                isDashRequested: input.IsDashRequested,
                dashTargetPosition: input.DashTargetPosition);
            return true;
        }

        protected override bool ShouldSimulate()
        {
            return ParentNetworkBehaviour.HasStateAuthority;
        }
    }
}
