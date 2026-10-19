using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "ProjectCore.TechnicalPrototype", null, "PlayerMovementStateMachine")]
    public sealed class PlayerEntityStateMachine : BaseEntityStateMachine<
            PlayerEntityState,
            BaseEntityState<PlayerEntityState, PlayerEntityPayload>,
            PlayerEntityPayload>
    {
        [SerializeField] private PlayerIdleState _idleState;
        [SerializeField] private PlayerLocomotionState _locomotionState;
        [SerializeField] private PlayerDodgeState _dodgeState;
        [SerializeField] private PlayerAttackState _attackState;
        [SerializeField] private PlayerInputSourceComponent _inputSource;
        private IInputBufferStateMutator _bufferState;

        [Inject]
        private void Construct(IInputBufferStateMutator bufferState) { _bufferState = bufferState; }

        public void CancelCurrentAction()
        {
            if (!IsComponentAvailable || (!ParentNetworkBehaviour.HasStateAuthority && !ParentNetworkBehaviour.HasInputAuthority)) return;
            _bufferState.ClearBufferedCommand();
            ChangeState(PlayerEntityState.Idle);
        }

        protected override PlayerEntityState InitialState => PlayerEntityState.Idle;

        public override Dictionary<
            PlayerEntityState,
            BaseEntityState<PlayerEntityState, PlayerEntityPayload>>
            CreateStatesDictionary()
        {
            var states = new Dictionary<PlayerEntityState, BaseEntityState<PlayerEntityState, PlayerEntityPayload>>
            {
                { PlayerEntityState.Idle, _idleState },
                { PlayerEntityState.Locomotion, _locomotionState },
            };
            if (_dodgeState != null) states.Add(PlayerEntityState.Dodge, _dodgeState);
            if (_attackState != null) states.Add(PlayerEntityState.Attack, _attackState);
            return states;
        }

        protected override bool TryGetPayload(out PlayerEntityPayload payload)
        {
            if (_inputSource == null)
            {
                payload = default;
                return false;
            }

            Vector2 direction = Vector2.zero;

            if (_inputSource.TryGetInput(out PlayerInputFrameData input))
            {
                direction = input.Direction;
            }

            payload = new(direction);
            return true;
        }

    }
}
