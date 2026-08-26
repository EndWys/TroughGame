using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class LocomotionState : BaseMovementState<PlayerMovementState, PlayerMovementPayload>
    {
        [SerializeField] private LocomotionMovementConfig _locomotionMovementConfig;
        [SerializeField] private TransformMovementBodyComponent _movementBody;
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
            if (_movementBody == null)
            {
                throw new InvalidOperationException("Locomotion state requires a movement body reference.");
            }

            if (_inputBufferController == null)
            {
                throw new InvalidOperationException("Locomotion state requires an input command controller.");
            }

            if (_facingDirectionMutator == null)
            {
                throw new InvalidOperationException("Locomotion state requires a facing direction mutator.");
            }

            if (_locomotionMovementConfig == null)
            {
                throw new InvalidOperationException(
                    "Locomotion state requires a locomotion movement config reference.");
            }

            return new IMovementStateProcessor<PlayerMovementState, PlayerMovementPayload>[]
            {
                new FourDirectionalFacingProcessor<PlayerMovementState, PlayerMovementPayload>(
                    facingDirectionMutator: _facingDirectionMutator),
                new DodgeTransitionProcessor<PlayerMovementState, PlayerMovementPayload>(
                    inputBufferController: _inputBufferController,
                    dodgeCommandId: MovementInputCommandConstants.DodgeCommandId,
                    dodgeMovementState: PlayerMovementState.Dodge),
                new LocomotionProcessor<PlayerMovementState, PlayerMovementPayload>(
                    movementBodyVelocityMutator: _movementBody,
                    locomotionMovementConfig: _locomotionMovementConfig),
                new NoDirectionTransitionProcessor<PlayerMovementState, PlayerMovementPayload>(
                    idleMovementState: PlayerMovementState.Idle),
            };
        }

        protected override PlayerMovementState FallbackState => PlayerMovementState.Locomotion;
    }
}
