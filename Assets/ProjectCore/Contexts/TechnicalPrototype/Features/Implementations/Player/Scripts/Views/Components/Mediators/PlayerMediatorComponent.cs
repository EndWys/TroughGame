using System;
using System.Collections.Generic;
using Domain;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerMediatorComponent :
        BaseNetworkEntityComponent,
        IPlayerMediator
    {
        [Header("COLLEAGUES")]
        [SerializeField] private PlayerAnimatorComponent _playerAnimatorComponent;

        private List<Action<HandlerPayload>> _handlers;

        public override void Init()
        {
            _handlers = new List<Action<HandlerPayload>>
            {
                HandleMovementStateChanged,
            };

        }

        public void Notify(HandlerPayload payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            foreach (Action<HandlerPayload> handler in _handlers)
            {
                handler.Invoke(payload);
            }
        }

        private void HandleMovementStateChanged(HandlerPayload payload)
        {
            if (payload is not PlayerMovementStateChangedPayload
                movementStateChangedPayload)
            {
                return;
            }

            switch (movementStateChangedPayload.CurrentMovementState)
            {
                case PlayerMovementState.Idle:
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
    }
}
