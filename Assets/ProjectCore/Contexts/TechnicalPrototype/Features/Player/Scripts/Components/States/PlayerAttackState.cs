using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerAttackState : BaseMovementState<PlayerMovementState, PlayerMovementPayload>
    {
        [SerializeField] private TransformMovementBodyComponent _movementBody;

        public override void Enter() { ((IMovementBodyVelocityMutator)_movementBody).Velocity = Vector2.zero; }
        public override void Exit() { }

        protected override IReadOnlyList<IMovementStateProcessor<PlayerMovementState, PlayerMovementPayload>>
            CreateMovementProcessors()
        {
            if (_movementBody == null) throw new InvalidOperationException("Attack state requires a movement body.");
            return Array.Empty<IMovementStateProcessor<PlayerMovementState, PlayerMovementPayload>>();
        }

        protected override PlayerMovementState FallbackState => PlayerMovementState.Attack;
    }
}
