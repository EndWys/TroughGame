using System;
using System.Collections.Generic;
using Domain;
using Fusion;
using UnityEngine;

namespace Prototype.Prototype
{
    public class PlayerMediator : NetworkBehaviour, IMediator<IPlayerColleague,EPlayerEventType>
    {
        [SerializeField] private PlayerCameraTracker _playerCameraTracker;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerDamageTaker _playerDamageTaker;
        [SerializeField] private PlayerHealth _playerHealth;
        
        private Dictionary<EPlayerEventType, Action<IPlayerColleague, object>> _handlers;

        public override void Spawned()
        {
            _playerCameraTracker.Init(this);
            _playerMovement.Init(this);
            _playerDamageTaker.Init(this);
            _playerHealth.Init(this);
            
            _handlers = new Dictionary<EPlayerEventType, Action<IPlayerColleague, object>>
            {
                { EPlayerEventType.OnPlayerMovementStateChange, HandleMovementStateChange },
                { EPlayerEventType.OnPlayerTakeDamage, HandleDamageTaken },
            };
        }

        public void Notify(IPlayerColleague sender, EPlayerEventType eventKey, object args = null)
        {
            if (_handlers.TryGetValue(eventKey, out var handler))
            {
                handler.Invoke(sender, args);
            }
        }

        private void HandleMovementStateChange(IPlayerColleague sender, object args = null)
        {
            if (args is MovementStateChangedPayload payload)
            {
                _playerCameraTracker.ChangeFieldOfView(payload);
            }
        }

        private void HandleDamageTaken(IPlayerColleague sender, object args = null)
        {
            if (args is DamageTakePayload payload)
            {
                _playerHealth.ApplyDamage(payload.Amount);
            }
        }
    }
}