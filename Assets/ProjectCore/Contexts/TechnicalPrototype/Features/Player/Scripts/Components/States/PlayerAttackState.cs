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
        private IPlayerInputDirectionAccessor _input;
        private IPlayerFacingDirectionMutator _facing;
        private IInputBufferController _buffer;
        private IInputBufferStateMutator _bufferState;
        [Inject]
        private void Construct(IMovementBodyVelocityMutator body, PlayerCombatActionComponent combat,
            IPlayerInputDirectionAccessor input, IPlayerFacingDirectionMutator facing,
            IInputBufferController buffer, IInputBufferStateMutator bufferState)
        {
            _body = body; _combat = combat; _input = input; _facing = facing;
            _buffer = buffer; _bufferState = bufferState;
        }
        public override void Enter()
        {
            _body.Velocity = Vector2.zero;
            _bufferState.LockInput();
            Vector2 direction = _input.LatestInputDirection;
            if (direction.sqrMagnitude <= 0f) direction = PlayerFacingDirectionUtility.ToVector(_facing.FacingDirection);
            _combat.StartAttack(direction);
            _facing.SetFacingDirection(PlayerFacingDirectionUtility.FromVector(direction, _facing.FacingDirection, 0.01f));
        }
        public override void Restore() { _body.Velocity = Vector2.zero; }
        public override void Exit() { _combat.StopForTransition(); _buffer.ReleaseCommandLock(); }
        public override PlayerEntityState Tick(PlayerEntityPayload payload)
        {
            if (_combat.ActionSnapshot.ActionId == 0) _bufferState.ClearBufferedCommand();
            return base.Tick(payload);
        }
        protected override IReadOnlyList<IStateProcessor<PlayerEntityState, PlayerEntityPayload>> CreateProcessors()
        {
            return new IStateProcessor<PlayerEntityState, PlayerEntityPayload>[]
            {
                CreateProcessor<PlayerActionTransitionProcessor>(PlayerInputActionConstants.AttackCommandId, PlayerEntityState.Attack, true, (Func<bool>)CanTransition),
                CreateProcessor<PlayerActionTransitionProcessor>(MovementInputCommandConstants.DodgeCommandId, PlayerEntityState.Dodge, false, (Func<bool>)CanTransition),
                CreateProcessor<PlayerAttackCompletionProcessor>(),
            };
        }
        protected override PlayerEntityState FallbackState => PlayerEntityState.Attack;
        private bool CanTransition() => _combat.ActionSnapshot.CanStartNextAction || _combat.ActionSnapshot.IsComplete;
    }
}
