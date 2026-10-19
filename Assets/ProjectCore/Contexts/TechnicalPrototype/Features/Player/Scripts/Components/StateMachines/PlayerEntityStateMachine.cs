using System;
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
        private IPlayerInputAccessor _input;
        private IInputBufferStateMutator _bufferState;

        [Inject]
        private void Construct(IInputBufferStateMutator bufferState, IPlayerInputAccessor input)
        {
            _bufferState = bufferState ?? throw new ArgumentNullException(nameof(bufferState));
            _input = input ?? throw new ArgumentNullException(nameof(input));
        }

        public void CancelCurrentAction()
        {
            if (!IsComponentAvailable)
                return;

            if (!HasSimulationAuthority)
                return;

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

            if (_dodgeState != null)
                states.Add(PlayerEntityState.Dodge, _dodgeState);

            if (_attackState != null)
                states.Add(PlayerEntityState.Attack, _attackState);

            return states;
        }

        protected override bool TryGetPayload(out PlayerEntityPayload payload)
        {
            payload = new PlayerEntityPayload(_input.Input);
            return true;
        }

    }
}
