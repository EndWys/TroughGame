using System;
using System.Collections.Generic;
using Shared;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerMediatorComponent : BaseNetworkEntityComponent, IPlayerMediator
    {
        [Header("COLLEAGUES")]
        [SerializeField] private PlayerAnimatorComponent _playerAnimatorComponent;

        private List<Action<HandlerPayload>> _handlers;

        protected override void OnInitialize()
        {
            _handlers = new List<Action<HandlerPayload>>
            {
                HandleEntityStateChanged,
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

        private void HandleEntityStateChanged(HandlerPayload payload)
        {
            if (payload is not PlayerEntityStateChangedPayload entityStateChangedPayload)
            {
                return;
            }

            switch (entityStateChangedPayload.CurrentEntityState)
            {
                case PlayerEntityState.Idle:
                case PlayerEntityState.Attack:
                    _playerAnimatorComponent.PlayIdle();
                    break;

                case PlayerEntityState.Locomotion:
                    _playerAnimatorComponent.PlayLocomotion();
                    break;

                case PlayerEntityState.Dodge:
                    _playerAnimatorComponent.PlayDodge();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(entityStateChangedPayload.CurrentEntityState),
                        entityStateChangedPayload.CurrentEntityState,
                        "Player entity state does not have a presentation handler.");
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
