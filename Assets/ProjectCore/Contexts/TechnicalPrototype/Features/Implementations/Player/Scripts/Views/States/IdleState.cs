using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class IdleState : BaseMovementState<PlayerMovementState, PlayerMovementPayload>
    {
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
            if (_inputBufferController == null)
            {
                throw new InvalidOperationException("Idle state requires an input buffer controller.");
            }

            return new IMovementStateProcessor<PlayerMovementState, PlayerMovementPayload>[]
            {
                new DodgeTransitionProcessor<PlayerMovementState, PlayerMovementPayload>(
                    inputBufferController: _inputBufferController,
                    dodgeCommandId: MovementInputCommandConstants.DodgeCommandId,
                    dodgeMovementState: PlayerMovementState.Dodge),
                new DirectionalLocomotionTransitionProcessor<PlayerMovementState, PlayerMovementPayload>(
                    locomotionMovementState: PlayerMovementState.Locomotion),
            };
        }

        protected override PlayerMovementState FallbackState => PlayerMovementState.Idle;
    }
}
