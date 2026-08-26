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
        private IPlayerFacingDirectionMutator _facingDirectionMutator;

        [Inject]
        private void Construct(
            IInputBufferController inputBufferController,
            IPlayerFacingDirectionMutator facingDirectionMutator)
        {
            _inputBufferController = inputBufferController;
            _facingDirectionMutator = facingDirectionMutator;
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

            if (_facingDirectionMutator == null)
            {
                throw new InvalidOperationException("Idle state requires a facing direction mutator.");
            }

            return new IMovementStateProcessor<PlayerMovementState, PlayerMovementPayload>[]
            {
                new FourDirectionalFacingProcessor<PlayerMovementState, PlayerMovementPayload>(
                    facingDirectionMutator: _facingDirectionMutator),
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
