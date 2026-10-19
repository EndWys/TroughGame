using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using Shared;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerAttackState : BaseEntityState<PlayerEntityState, PlayerEntityPayload>
    {
        private IMovementBodyVelocityMutator _body;
        private PlayerCombatActionComponent _combat;
        private IPlayerInputAccessor _input;
        private IPlayerFacingDirectionMutator _facing;
        private IInputBufferController _buffer;
        private IInputBufferStateMutator _bufferState;

        [Inject]
        private void Construct(
            IMovementBodyVelocityMutator body,
            PlayerCombatActionComponent combat,
            IPlayerInputAccessor input,
            IPlayerFacingDirectionMutator facing,
            IInputBufferController buffer,
            IInputBufferStateMutator bufferState)
        {
            _body = body ?? throw new ArgumentNullException(nameof(body));
            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _facing = facing ?? throw new ArgumentNullException(nameof(facing));
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            _bufferState = bufferState ?? throw new ArgumentNullException(nameof(bufferState));
        }

        public override void Enter()
        {
            _body.Velocity = Vector2.zero;
            _bufferState.LockInput();
            Vector2 direction = _input.Input.LastNonZeroDirection;

            if (direction.sqrMagnitude <= 0f)
                direction = PlayerFacingDirectionUtility.ToVector(_facing.FacingDirection);

            _combat.StartAttack(direction);

            _facing.SetFacingDirection(
                PlayerFacingDirectionUtility.FromVector(
                    direction,
                    _facing.FacingDirection,
                    0.01f));
        }

        public override void Restore() => _body.Velocity = Vector2.zero;

        public override void Exit()
        {
            _combat.StopForTransition();
            _buffer.ReleaseCommandLock();
        }

        public override PlayerEntityState Tick(PlayerEntityPayload payload)
        {
            if (_combat.ActionSnapshot.ActionId == 0)
                _bufferState.ClearBufferedCommand();

            return base.Tick(payload);
        }

        protected override IReadOnlyList<IStateProcessor<PlayerEntityState, PlayerEntityPayload>>
            CreateProcessors()
        {
            return new IStateProcessor<PlayerEntityState, PlayerEntityPayload>[]
            {
                CreateProcessor<PlayerActionTransitionProcessor>(
                    PlayerInputActionConstants.AttackCommandId,
                    PlayerEntityState.Attack,
                    true,
                    (Func<bool>)CanTransition),

                CreateProcessor<PlayerActionTransitionProcessor>(
                    MovementInputCommandConstants.DodgeCommandId,
                    PlayerEntityState.Dodge,
                    false,
                    (Func<bool>)CanTransition),

                CreateProcessor<PlayerAttackCompletionProcessor>(),
            };
        }

        protected override PlayerEntityState FallbackState => PlayerEntityState.Attack;

        private bool CanTransition() =>
            _combat.ActionSnapshot.CanStartNextAction || _combat.ActionSnapshot.IsComplete;
    }
}
