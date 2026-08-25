using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class LocomotionState :
        BaseMovementState<PlayerMovementState, PlayerMovementPayload>
    {
        [SerializeField]
        private LocomotionMovementConfig _locomotionConfig;
        [SerializeField]
        private TransformMovementBodyComponent _movementBody;
        private IInputBufferController _inputBufferController;

        [Inject]
        private void Construct(IInputBufferController inputBufferController)
        {
            _inputBufferController = inputBufferController;
        }

        public override void Enter() { }

        public override void Exit() { }

        protected override IReadOnlyList<IMovementStateProcessor<PlayerMovementState, PlayerMovementPayload>>
            CreateMovementProcessors()
        {
            if (_movementBody == null)
            {
                throw new InvalidOperationException(
                    "Locomotion state requires a movement body reference.");
            }

            if (_inputBufferController == null)
            {
                throw new InvalidOperationException(
                    "Locomotion state requires an input command controller.");
            }

            return new IMovementStateProcessor<
                PlayerMovementState,
                PlayerMovementPayload>[]
            {
                new DodgeTransitionProcessor<
                    PlayerMovementState,
                    PlayerMovementPayload>(
                    _inputBufferController,
                    MovementInputCommandConstants.DodgeCommandId,
                    PlayerMovementState.Dodge),
                new LocomotionProcessor<
                    PlayerMovementState,
                    PlayerMovementPayload>(
                    _movementBody,
                    _locomotionConfig),
            };
        }

        protected override PlayerMovementState FallbackState =>
            PlayerMovementState.Locomotion;
    }
}
