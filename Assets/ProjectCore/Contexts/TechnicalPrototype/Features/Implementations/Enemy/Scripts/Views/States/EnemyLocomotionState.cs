using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyLocomotionState :
        BaseMovementState<EnemyMovementState, EnemyMovementPayload>
    {
        [SerializeField] private LocomotionMovementConfig _locomotionMovementConfig;
        [SerializeField] private TransformMovementBodyComponent _movementBody;

        public override void Enter() { }

        public override void Exit() { }

        protected override IReadOnlyList<IMovementStateProcessor<EnemyMovementState, EnemyMovementPayload>>
            CreateMovementProcessors()
        {
            if (_movementBody == null)
            {
                throw new InvalidOperationException("Enemy locomotion state requires a movement body reference.");
            }

            if (_locomotionMovementConfig == null)
            {
                throw new InvalidOperationException(
                    "Enemy locomotion state requires a locomotion movement config reference.");
            }

            return new IMovementStateProcessor<EnemyMovementState, EnemyMovementPayload>[]
            {
                new LocomotionProcessor<EnemyMovementState, EnemyMovementPayload>(
                    movementBodyVelocityMutator: _movementBody,
                    locomotionMovementConfig: _locomotionMovementConfig),
                new NoDirectionTransitionProcessor<EnemyMovementState, EnemyMovementPayload>(
                    idleMovementState: EnemyMovementState.Idle),
            };
        }

        protected override EnemyMovementState FallbackState => EnemyMovementState.Locomotion;
    }
}
