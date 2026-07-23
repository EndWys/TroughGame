using System;
using System.Collections.Generic;
using Domain;
using ProjectCore.GameCore;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectCore.Prototype
{
    public class PlayerMediatorComponent :
        BaseNetworkEntityCompositeComponent,
        IPlayerMediator
    {
        [Header("COMPONENTS")]
        [FormerlySerializedAs("_playerCameraTracker")]
        [FormerlySerializedAs("_playerCameraController")]
        [SerializeField] private PlayerCameraComponent _playerCameraComponent;
        [SerializeField] private PlayerMovementStateMachine _playerMovement;
        [FormerlySerializedAs("_playerDamageTaker")]
        [SerializeField] private PlayerDamageTriggerComponent _playerDamageTrigger;
        [FormerlySerializedAs("_playerHealth")]
        [SerializeField] private PlayerHealthComponent _playerHealthComponent;

        private List<Action<HandlerPayload>> _handlers;

        public override void Init()
        {
            _handlers = new List<Action<HandlerPayload>>
            {
                HandleMovementStateChange,
                HandleDamageTaken,
            };

            base.Init();
        }

        public void Notify(HandlerPayload payload)
        {
            foreach (Action<HandlerPayload> handler in _handlers)
            {
                handler.Invoke(payload);
            }
        }

        protected override IEnumerable<INetworkEntityComponent> CreateComponents()
        {
            return new INetworkEntityComponent[]
            {
                _playerCameraComponent,
                _playerMovement,
                _playerDamageTrigger,
                _playerHealthComponent,
            };
        }

        private void HandleMovementStateChange(HandlerPayload payload)
        {
            if (payload is PlayerMovementStateChangedPayload playerMovementStateChangedPayload)
            {
                _playerCameraComponent.ChangeFieldOfView(playerMovementStateChangedPayload);
            }
        }

        private void HandleDamageTaken(HandlerPayload payload)
        {
            if (payload is PlayerDamageTakenPayload playerDamageTakenPayload)
            {
                _playerHealthComponent.ApplyDamage(playerDamageTakenPayload.Amount);
            }
        }
    }
}
