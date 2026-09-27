using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;

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

        protected override EnemyMovementState InitialState => EnemyMovementState.Idle;

        protected override Dictionary<
            EnemyMovementState,
            BaseMovementState<EnemyMovementState, EnemyMovementPayload>>
            CreateMovementStatesDictionary()
        {
            return new Dictionary<
                EnemyMovementState,
                BaseMovementState<EnemyMovementState, EnemyMovementPayload>>
            {
                { EnemyMovementState.Idle, _idleState },
                { EnemyMovementState.Locomotion, _locomotionState },
            };
        }

        protected override bool TryGetMovementPayload(out EnemyMovementPayload payload)
        {
            Vector2 direction = Vector2.zero;

            if (_inputSource != null && _inputSource.TryGetInput(out EnemyInputFrameData input))
            {
                direction = input.Direction;
            }

            payload = new EnemyMovementPayload(direction);
            return true;
        }

        protected override bool ShouldSimulateMovement()
        {
            return ParentNetworkBehaviour.HasStateAuthority;
        }
    }
}
