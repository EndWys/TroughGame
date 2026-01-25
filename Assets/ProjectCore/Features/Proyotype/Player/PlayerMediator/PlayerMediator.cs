using System;
using System.Collections.Generic;
using Fusion;
using ProjectCore.Domain.Scripts.Paterns.Mediator;
using ProjectCore.Features.Prototype.Player.PlayerMediator.EventPayloads;
using UnityEngine;

namespace ProjectCore.Features.Prototype.Player.PlayerMediator
{
    public class PlayerMediator : NetworkBehaviour, IMediator<IPlayerColleague,EPlayerEventType>
    {
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerDamageTaker _playerDamageTaker;
        [SerializeField] private PlayerHealth _playerHealth;
        
        private Dictionary<EPlayerEventType, Action<IPlayerColleague, object>> _handlers;

        public override void Spawned()
        {
            _playerMovement.Initialize(this);
            _playerDamageTaker.Initialize(this);
            _playerHealth.Initialize(this);
            
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
                //_playerHealth.ApplyDamage(payload.Amount);
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