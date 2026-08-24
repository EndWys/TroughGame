using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class LocomotionState :
        BaseMovementState<PlayerMovementState, PlayerMovementPayload>
    {
        [SerializeField]
        private LocomotionMovementConfig _locomotionConfig;
        [SerializeField]
        private TransformMovementBodyComponent _movementBody;

        public override void Enter() { }

        public override void Exit() { }

        protected override IReadOnlyList<
            IMovementStateProcessor<PlayerMovementState, PlayerMovementPayload>>
            CreateMovementProcessors()
        {
            if (_movementBody == null)
            {
                throw new InvalidOperationException(
                    "Locomotion state requires a movement body reference.");
            }

            return new IMovementStateProcessor<
                PlayerMovementState,
                PlayerMovementPayload>[]
            {
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
