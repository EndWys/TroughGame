using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using Shared;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerLocomotionState : BaseEntityState<PlayerEntityState, PlayerEntityPayload>
    {
        [SerializeField] private LocomotionMovementConfig _locomotionMovementConfig;

        private IMovementBodyVelocityMutator _body;

        [Inject]
        private void Construct(IMovementBodyVelocityMutator body)
        {
            _body = body ?? throw new ArgumentNullException(nameof(body));
        }

        public override void Enter()
        {
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

                CreateProcessor<LocomotionProcessor<PlayerEntityState, PlayerEntityPayload>>(
                    _locomotionMovementConfig),

                CreateProcessor<NoDirectionTransitionProcessor<PlayerEntityState, PlayerEntityPayload>>(
                    PlayerEntityState.Idle),
            };
        }

        protected override PlayerEntityState FallbackState => PlayerEntityState.Locomotion;
    }
}
