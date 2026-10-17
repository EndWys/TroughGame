using System;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerActionControllerComponent : BaseNetworkEntityComponent
    {
        [SerializeField] private PlayerInputSourceComponent _inputSource;
        [SerializeField] private PlayerCombatActionComponent _combatAction;
        [SerializeField] private PlayerMovementStateMachine _movementStateMachine;
        [SerializeField] private PlayerDodgeState _dodgeState;

        private IInputBufferController _buffer;
        private IMovementStateDataAccessor<PlayerMovementState> _movement;
        private ICombatActionMutator _actionState;
        private IPlayerFacingDirectionAccessor _facing;

        [Inject]
        private void Construct(IInputBufferController buffer,
            IMovementStateDataAccessor<PlayerMovementState> movement,
            IPlayerFacingDirectionAccessor facing,
            [InjectOptional] ICombatActionMutator actionState)
        {
            _buffer = buffer;
            _movement = movement;
            _facing = facing;
            _actionState = actionState;
        }

        protected override void OnInitialize()
        {
            if (_inputSource == null || _movementStateMachine == null)
                throw new InvalidOperationException("Player action controller requires input and movement colleagues.");
        }

        protected override void OnNetworkTick()
        {
            if ((!ParentNetworkBehaviour.HasStateAuthority && !ParentNetworkBehaviour.HasInputAuthority) ||
                !_movementStateMachine.IsComponentAvailable) return;
            if (_actionState != null && _inputSource.TryGetInput(out PlayerInputFrameData input) &&
                input.Direction.sqrMagnitude > 0f) _actionState.SetLatestInputDirection(input.Direction.normalized);

            // Querying also expires a pending intent while the actor is locked.
            bool attackPending = _buffer.HasBufferedCommand(PlayerInputActionConstants.AttackCommandId);
            bool dodgePending = _buffer.HasBufferedCommand(MovementInputCommandConstants.DodgeCommandId);
            if (_movement.CurrentMovementStates == PlayerMovementState.Dodge) return;
            if (_movement.CurrentMovementStates == PlayerMovementState.Attack &&
                (_combatAction == null || !_combatAction.CanStartAttack)) return;

            if (attackPending && _combatAction != null && _combatAction.CanStartAttack)
            {
                _buffer.EndAction();
                if (!_buffer.BeginAction(PlayerInputActionConstants.AttackCommandId)) return;
                Vector2 direction = _actionState?.LatestInputDirection ?? Vector2.zero;
                if (direction.sqrMagnitude <= 0f) direction = PlayerFacingDirectionUtility.ToVector(_facing.FacingDirection);
                _combatAction.StartAttack(direction);
            }
            else if (dodgePending && _dodgeState != null)
            {
                if (_combatAction != null && _combatAction.ActionSnapshot.ActionId != 0)
                    _combatAction.StopForTransition();
                _buffer.EndAction();
                if (_buffer.BeginAction(MovementInputCommandConstants.DodgeCommandId))
                    _movementStateMachine.ChangeState(PlayerMovementState.Dodge);
            }
        }
    }
}
