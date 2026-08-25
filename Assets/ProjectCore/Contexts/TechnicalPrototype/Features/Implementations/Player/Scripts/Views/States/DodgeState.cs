using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class DodgeState :
        BaseMovementState<PlayerMovementState, PlayerMovementPayload>
    {
        [SerializeField]
        private TransformMovementBodyComponent _movementBody;
        [SerializeField]
        private DodgeMovementConfig _dodgeConfig;

        private IInputBufferController _inputBufferController;
        private IMovementStateTimerMutator _movementStateTimerMutator;

        [Inject]
        private void Construct(
            IInputBufferController inputBufferController,
            IMovementStateTimerMutator movementStateTimerMutator)
        {
            _inputBufferController = inputBufferController;
            _movementStateTimerMutator = movementStateTimerMutator;
        }

        public override void Enter()
        {
            if (_inputBufferController == null ||
                _movementStateTimerMutator == null ||
                _dodgeConfig == null)
            {
                throw new InvalidOperationException(
                    "Dodge state is not configured for the input command controller.");
            }

            if (!_inputBufferController.BeginAction(
                    MovementInputCommandConstants.DodgeCommandId))
            {
                throw new InvalidOperationException(
                    "Dodge state entered without a pending Dodge command.");
            }

            _movementStateTimerMutator.StartStateTimer(
                _dodgeConfig.DurationSeconds);
        }

        public override void Exit()
        {
            _inputBufferController?.EndAction();
            _movementStateTimerMutator?.StopStateTimer();
        }

        protected override IReadOnlyList<
            IMovementStateProcessor<PlayerMovementState, PlayerMovementPayload>>
            CreateMovementProcessors()
        {
            if (_movementBody == null)
            {
                throw new InvalidOperationException(
                    "Dodge state requires a movement body reference.");
            }

            if (_inputBufferController == null)
            {
                throw new InvalidOperationException(
                    "Dodge state requires an input command controller.");
            }

            if (_movementStateTimerMutator == null)
            {
                throw new InvalidOperationException(
                    "Dodge state requires movement state timing.");
            }

            if (_dodgeConfig == null)
            {
                throw new InvalidOperationException(
                    "Dodge state requires a Dodge movement config reference.");
            }

            return new IMovementStateProcessor<
                PlayerMovementState,
                PlayerMovementPayload>[]
            {
                new DodgeProcessor<
                    PlayerMovementState,
                    PlayerMovementPayload>(
                    _movementBody,
                    _movementStateTimerMutator,
                    _dodgeConfig,
                    PlayerMovementState.Locomotion),
            };
        }

        protected override PlayerMovementState FallbackState =>
            PlayerMovementState.Dodge;
    }
}
