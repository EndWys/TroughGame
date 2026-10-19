using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using Shared;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerIdleState : BaseEntityState<PlayerEntityState, PlayerEntityPayload>
    {
        private IMovementBodyVelocityMutator _body;

        [Inject]
        private void Construct(IMovementBodyVelocityMutator body)
        {
            _body = body ?? throw new ArgumentNullException(nameof(body));
        }

        public override void Enter()
        {
            _body.Velocity = Vector2.zero;
        }

        public override void Restore()
        {
            _body.Velocity = Vector2.zero;
        }

        public override void Exit()
        {
        }

        protected override IReadOnlyList<IStateProcessor<PlayerEntityState, PlayerEntityPayload>>
            CreateProcessors()
        {
            return new IStateProcessor<PlayerEntityState, PlayerEntityPayload>[]
            {
                CreateProcessor<PlayerActionTransitionProcessor>(
                    PlayerInputActionConstants.AttackCommandId,
                    PlayerEntityState.Attack,
                    false,
                    (Func<bool>)(() => true)),

                CreateProcessor<PlayerActionTransitionProcessor>(
                    MovementInputCommandConstants.DodgeCommandId,
                    PlayerEntityState.Dodge,
                    false,
                    (Func<bool>)(() => true)),

                CreateProcessor<PlayerFourDirectionalFacingProcessor<PlayerEntityState, PlayerEntityPayload>>(),

                CreateProcessor<DirectionalLocomotionTransitionProcessor<PlayerEntityState, PlayerEntityPayload>>(
                    PlayerEntityState.Locomotion),
            };
        }

        protected override PlayerEntityState FallbackState => PlayerEntityState.Idle;
    }
}
