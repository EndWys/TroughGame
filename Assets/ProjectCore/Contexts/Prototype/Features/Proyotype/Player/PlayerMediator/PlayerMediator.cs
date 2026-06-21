using System;
using System.Collections.Generic;
using Domain;
using UnityEngine;

namespace Prototype.Prototype
{
    public class PlayerMediator :
        BaseNetworkEntityCompositeComponent,
        IPlayerMediator
    {
        [Header("COMPONENTS")]
        [SerializeField] private PlayerCameraTracker _playerCameraTracker;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerDamageTaker _playerDamageTaker;
        [SerializeField] private PlayerHealth _playerHealth;

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
                _playerCameraTracker,
                _playerMovement,
                _playerDamageTaker,
                _playerHealth,
            };
        }

        private void HandleMovementStateChange(HandlerPayload payload)
        {
            if (payload is MovementStateChangedPayload movementStateChangedPayload)
            {
                _playerCameraTracker.ChangeFieldOfView(movementStateChangedPayload);
            }
        }

        private void HandleDamageTaken(HandlerPayload payload)
        {
            if (payload is DamageTakePayload damageTakePayload)
            {
                _playerHealth.ApplyDamage(damageTakePayload.Amount);
            }
        }
    }
}
