using System;
using System.Collections.Generic;
using Shared;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerMediatorComponent : BaseNetworkEntityComponent, IPlayerMediator
    {
        [Header("COLLEAGUES")]
        [SerializeField] private PlayerAnimatorComponent _playerAnimatorComponent;

        [SerializeField] private PlayerMovementStateMachine _movementStateMachine;

        private IInputBufferController _buffer;
        private IInputBufferStateMutator _bufferState;
        private IMovementStateDataAccessor<PlayerMovementState> _movement;
        private ICombatInvulnerabilityMutator _invulnerability;
        private List<Action<HandlerPayload>> _handlers;

        [Inject]
        private void Construct(IInputBufferController buffer, IInputBufferStateMutator bufferState,
            IMovementStateDataAccessor<PlayerMovementState> movement,
            [InjectOptional] ICombatInvulnerabilityMutator invulnerability)
        {
            _buffer = buffer;
            _bufferState = bufferState;
            _movement = movement;
            _invulnerability = invulnerability;
        }

        protected override void OnInitialize()
        {
            if (_movementStateMachine == null)
                throw new InvalidOperationException("Player mediator requires a movement colleague.");
            _handlers = new List<Action<HandlerPayload>>
            {
                HandleAttackState,
                HandleDodgePhase,
                HandleMovementStateChanged,
                HandleFacingDirectionChanged,
            };
        }

        public void Notify(HandlerPayload payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (!IsComponentAvailable) return;

            foreach (Action<HandlerPayload> handler in _handlers)
            {
                handler.Invoke(payload);
            }
        }

        protected override void OnResetLocalRuntime() { _handlers = null; }

        private void HandleAttackState(HandlerPayload payload)
        {
            if (payload is not PlayerAttackStatePayload attack || !_movementStateMachine.IsComponentAvailable) return;
            if (attack.Active)
            {
                if (attack.Restarted || _movement.CurrentMovementStates != PlayerMovementState.Attack)
                    _movementStateMachine.ChangeState(PlayerMovementState.Attack);
            }
            else
            {
                if (attack.ClearInput) _bufferState.ClearBufferedCommand();
                if (_movement.CurrentMovementStates == PlayerMovementState.Attack)
                    _movementStateMachine.ChangeState(PlayerMovementState.Idle);
                if (_movement.CurrentMovementStates != PlayerMovementState.Dodge) _buffer.EndAction();
            }
        }

        private void HandleDodgePhase(HandlerPayload payload)
        {
            if (payload is not PlayerDodgePhasePayload dodge) return;
            _invulnerability?.SetInvulnerabilityUntilTick(
                dodge.Phase == DodgeMovementPhase.Active ? dodge.ActiveEndTick : null);
            if (dodge.Phase == DodgeMovementPhase.None) _buffer.EndAction();
        }

        private void HandleMovementStateChanged(HandlerPayload payload)
        {
            if (payload is not PlayerMovementStateChangedPayload movementStateChangedPayload)
            {
                return;
            }

            switch (movementStateChangedPayload.CurrentMovementState)
            {
                case PlayerMovementState.Idle:
                case PlayerMovementState.Attack:
                    _playerAnimatorComponent.PlayIdle();
                    break;

                case PlayerMovementState.Locomotion:
                    _playerAnimatorComponent.PlayLocomotion();
                    break;

                case PlayerMovementState.Dodge:
                    _playerAnimatorComponent.PlayDodge();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(movementStateChangedPayload.CurrentMovementState),
                        movementStateChangedPayload.CurrentMovementState,
                        "Player movement state does not have a presentation handler.");
            }
        }

        private void HandleFacingDirectionChanged(HandlerPayload payload)
        {
            if (payload is PlayerFacingDirectionChangedPayload facingDirectionChangedPayload)
            {
                _playerAnimatorComponent.SetFacingDirection(facingDirectionChangedPayload.FacingDirection);
            }
        }
    }
}
