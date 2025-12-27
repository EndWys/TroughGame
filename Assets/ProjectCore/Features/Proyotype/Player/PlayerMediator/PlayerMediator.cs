using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using ProjectCore.Domain.Scripts.Paterns.Mediator;
using UnityEngine;

namespace ProjectCore.Features.Proyotype.Player.PlayerMediator
{
    public class PlayerMediator : NetworkBehaviour, IMediator<IPlayerColleague,EPlayerEventType>
    {
        [SerializeField] private PlayerMovement _playerMovement;
        
        private Dictionary<EPlayerEventType, Action<IPlayerColleague, object>> _handlers;

        public override void Spawned()
        {
            _playerMovement.Initialize(this);
            
            _handlers = new Dictionary<EPlayerEventType, Action<IPlayerColleague, object>>
            {
                { EPlayerEventType.OnPlayerWalk, OnPlayerWalk },
            };
        }

        public void Notify(IPlayerColleague sender, EPlayerEventType eventKey, object args = null)
        {
            if (_handlers.TryGetValue(eventKey, out var handler))
            {
                handler.Invoke(sender, args);
            }
        }

        private void OnPlayerWalk(IPlayerColleague sender, object args = null)
        {
            Debug.Log("OnPlayerWalk");
        }
    }
}