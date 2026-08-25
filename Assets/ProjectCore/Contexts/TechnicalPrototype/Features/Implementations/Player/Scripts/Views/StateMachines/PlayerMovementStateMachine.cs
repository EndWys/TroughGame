using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerMovementStateMachine :
        BaseMovementStateMachine<
            PlayerMovementState,
            BaseMovementState<PlayerMovementState, PlayerMovementPayload>,
            PlayerMovementPayload>
    {
        [SerializeField] private IdleState _idleState;
        [SerializeField] private LocomotionState _locomotionState;
        [SerializeField] private DodgeState _dodgeState;
        [SerializeField] private PlayerInputSourceComponent _inputSource;

        protected override PlayerMovementState InitialState =>
            PlayerMovementState.Idle;

        protected override Dictionary<
            PlayerMovementState,
            BaseMovementState<PlayerMovementState, PlayerMovementPayload>>
            CreateMovementStatesDictionary()
        {
            return new Dictionary<
                PlayerMovementState,
                BaseMovementState<PlayerMovementState, PlayerMovementPayload>>
            {
                { PlayerMovementState.Idle, _idleState },
                { PlayerMovementState.Locomotion, _locomotionState },
                { PlayerMovementState.Dodge, _dodgeState },
            };
        }

        protected override bool TryGetMovementPayload(
            out PlayerMovementPayload payload)
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

            payload = new PlayerMovementPayload(direction);
            return true;
        }

    }
}
