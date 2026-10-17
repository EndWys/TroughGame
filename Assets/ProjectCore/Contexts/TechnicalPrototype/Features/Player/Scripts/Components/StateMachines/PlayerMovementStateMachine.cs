using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerMovementStateMachine : BaseMovementStateMachine<
            PlayerMovementState,
            BaseMovementState<PlayerMovementState, PlayerMovementPayload>,
            PlayerMovementPayload>
    {
        [SerializeField] private PlayerIdleState _idleState;
        [SerializeField] private PlayerLocomotionState _locomotionState;
        [SerializeField] private PlayerDodgeState _dodgeState;
        [SerializeField] private PlayerAttackState _attackState;
        [SerializeField] private PlayerInputSourceComponent _inputSource;
        private PlayerMovementState? _enteredState;
        private IMovementStateDataMutator<PlayerMovementState> _movement;

        [Inject]
        private void Construct(IMovementStateDataMutator<PlayerMovementState> movement)
        {
            _movement = movement;
        }

        protected override void BeforeMovementUpdate(PlayerMovementPayload payload)
        {
            // Replicated state can rewind while the local state objects retain their later activation.
            if (_enteredState != _movement.CurrentMovementStates)
            {
                PlayerMovementState restored = _movement.CurrentMovementStates;
                // The network transition already happened. Do not run Exit or mutate its restored timer/lock.
                // Dodge derives velocity from that timer during Tick; its ordinary Enter starts a new execution.
                if (restored != PlayerMovementState.Dodge) States[restored].Enter();
                _enteredState = restored;
            }
        }

        protected override void AfterNextStateEnter() { _enteredState = _movement.CurrentMovementStates; }
        protected override void OnResetLocalRuntime() { _enteredState = null; }

        protected override PlayerMovementState InitialState => PlayerMovementState.Idle;

        protected override Dictionary<
            PlayerMovementState,
            BaseMovementState<PlayerMovementState, PlayerMovementPayload>>
            CreateMovementStatesDictionary()
        {
            var states = new Dictionary<PlayerMovementState, BaseMovementState<PlayerMovementState, PlayerMovementPayload>>
            {
                { PlayerMovementState.Idle, _idleState },
                { PlayerMovementState.Locomotion, _locomotionState },
            };
            if (_dodgeState != null) states.Add(PlayerMovementState.Dodge, _dodgeState);
            if (_attackState != null) states.Add(PlayerMovementState.Attack, _attackState);
            return states;
        }

        protected override bool TryGetMovementPayload(out PlayerMovementPayload payload)
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
