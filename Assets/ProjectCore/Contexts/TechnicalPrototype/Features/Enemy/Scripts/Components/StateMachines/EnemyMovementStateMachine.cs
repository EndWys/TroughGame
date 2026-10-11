using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyMovementStateMachine : BaseMovementStateMachine<
        EnemyMovementState,
        BaseMovementState<EnemyMovementState, EnemyMovementPayload>,
        EnemyMovementPayload>
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

        protected override EnemyMovementState InitialState => EnemyMovementState.Idle;

        protected override Dictionary<
            EnemyMovementState,
            BaseMovementState<EnemyMovementState, EnemyMovementPayload>>
            CreateMovementStatesDictionary()
        {
            var movementStates = new Dictionary<
                EnemyMovementState,
                BaseMovementState<EnemyMovementState, EnemyMovementPayload>>
            {
                { EnemyMovementState.Idle, _idleState },
                { EnemyMovementState.Locomotion, _locomotionState },
            };

            foreach (IEnemyMovementAbilityContributor ability in
                     _abilityCollection.GetAbilities<IEnemyMovementAbilityContributor>())
            {
                ability.AddMovementStates(movementStates);
            }

            return movementStates;
        }

        protected override bool TryGetMovementPayload(out EnemyMovementPayload payload)
        {
            EnemyInputFrameData input = default;

            if (_inputSource != null)
            {
                _inputSource.TryGetInput(out input);
            }

            payload = new EnemyMovementPayload(
                direction: input.Direction,
                isDashRequested: input.IsDashRequested,
                dashTargetPosition: input.DashTargetPosition);
            return true;
        }

        protected override bool ShouldSimulateMovement()
        {
            return ParentNetworkBehaviour.HasStateAuthority;
        }
    }
}
